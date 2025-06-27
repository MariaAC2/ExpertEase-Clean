using Stripe;
using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;
using ExpertEase.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;
using ExpertEase.Application.DataTransferObjects.StripeAccountDTOs;

namespace ExpertEase.Infrastructure.Services;

/// <summary>
/// Enhanced PaymentService with full escrow support
/// Handles payment creation, confirmation, escrow management, transfers, and refunds
/// </summary>
public class PaymentService(
    IRepository<WebAppDatabaseContext> repository,
    IStripeAccountService stripeAccountService,
    IProtectionFeeConfigurationService feeConfigurationService,
    ILogger<PaymentService> logger)
    : IPaymentService
{
    #region Payment Intent Creation & Confirmation

    /// <summary>
    /// Creates payment intent with escrow - money will be held on platform until service completion
    /// </summary>
    public async Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentIntent(
        PaymentIntentCreateDTO createDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("🚀 Creating ESCROW payment intent for ReplyId: {ReplyId}", createDTO.ReplyId);
            logger.LogInformation("💰 Amounts - Service: {ServiceAmount}, Fee: {ProtectionFee}, Total: {TotalAmount}",
                createDTO.ServiceAmount, createDTO.ProtectionFee, createDTO.TotalAmount);

            // ✅ Step 1: Validate input amounts
            var validationResult = ValidateOrCalculateAmounts(createDTO);
            if (!validationResult.IsSuccess)
                return validationResult;

            // ✅ Step 2: Get and validate reply and specialist
            var (reply, specialist, specialistAccountId) = await GetAndValidateReplyData(createDTO.ReplyId, cancellationToken);
            if (reply == null || string.IsNullOrEmpty(specialistAccountId))
            {
                return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                    HttpStatusCode.BadRequest,
                    "Reply not found or specialist doesn't have a Stripe account configured",
                    ErrorCodes.EntityNotFound));
            }

            // ✅ Step 3: Create Stripe payment intent (ESCROW MODE)
            var stripeResponse = await CreateStripePaymentIntent(createDTO, specialistAccountId);

            // ✅ Step 4: Create payment record in database
            var payment = await CreatePaymentRecord(createDTO, reply, specialistAccountId, stripeResponse.PaymentIntentId, cancellationToken);

            logger.LogInformation("✅ ESCROW payment intent created successfully: {PaymentIntentId}", stripeResponse.PaymentIntentId);

            // ✅ Step 5: Return response with all required fields
            return ServiceResponse.CreateSuccessResponse(new PaymentIntentResponseDTO
            {
                ClientSecret = stripeResponse.ClientSecret,
                PaymentIntentId = stripeResponse.PaymentIntentId,
                StripeAccountId = stripeResponse.StripeAccountId,
                ServiceAmount = stripeResponse.ServiceAmount,
                ProtectionFee = stripeResponse.ProtectionFee,
                TotalAmount = stripeResponse.TotalAmount,
                ProtectionFeeDetails = createDTO.ProtectionFeeDetails
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating payment intent for ReplyId: {ReplyId}", createDTO.ReplyId);
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                HttpStatusCode.InternalServerError,
                $"Payment intent creation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    /// <summary>
    /// Confirms payment after successful Stripe processing - money goes into escrow
    /// </summary>
    public async Task<ServiceResponse> ConfirmPayment(
        PaymentConfirmationDTO confirmationDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("🔒 Confirming payment for escrow: {PaymentIntentId}", confirmationDTO.PaymentIntentId);

            // ✅ Step 1: Get payment record
            var payment = await GetPaymentByStripeId(confirmationDTO.PaymentIntentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            // ✅ Step 2: Verify payment with Stripe
            var stripePaymentIntent = await VerifyStripePaymentStatus(confirmationDTO.PaymentIntentId, cancellationToken);
            if (stripePaymentIntent is not { Status: "succeeded" })
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Payment verification failed. Stripe status: {stripePaymentIntent?.Status ?? "unknown"}",
                    ErrorCodes.TechnicalError));
            }

            // ✅ Step 3: Update payment to ESCROWED status
            await UpdatePaymentToEscrowed(payment, stripePaymentIntent, cancellationToken);

            // ✅ Step 4: Create service task (now that payment is secured)
            await CreateServiceTaskFromPayment(payment, cancellationToken);

            logger.LogInformation("✅ Payment confirmed and escrowed: {PaymentIntentId}", confirmationDTO.PaymentIntentId);
            logger.LogInformation("💰 Amounts - Total: {TotalAmount}, Service: {ServiceAmount}, Platform Fee: {ProtectionFee}",
                payment.TotalAmount, payment.ServiceAmount, payment.ProtectionFee);

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error confirming payment: {PaymentIntentId}", confirmationDTO.PaymentIntentId);
            return ServiceResponse.CreateErrorResponse(new(
                HttpStatusCode.InternalServerError,
                $"Payment confirmation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    #endregion

    #region Escrow Management

    /// <summary>
    /// Release escrowed payment to specialist when service is completed
    /// </summary>
    public async Task<ServiceResponse> ReleasePayment(
        PaymentReleaseDTO releaseDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("🚀 Releasing payment to specialist: {PaymentId}", releaseDTO.PaymentId);

            // ✅ Step 1: Get and validate payment
            var payment = await GetPaymentById(releaseDTO.PaymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (!payment.CanBeReleased())
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Payment cannot be released. Current status: {payment.Status.GetStatusMessage()}",
                    ErrorCodes.CannotUpdate));
            }

            // ✅ Step 2: Determine transfer amount
            var amountToTransfer = releaseDTO.CustomAmount ?? payment.ServiceAmount;
            if (amountToTransfer > payment.ServiceAmount || amountToTransfer <= 0)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Invalid transfer amount. Must be between 0 and {payment.ServiceAmount} RON",
                    ErrorCodes.CannotUpdate));
            }

            // ✅ Step 3: Transfer money to specialist via Stripe
            var transferResult = await stripeAccountService.TransferToSpecialist(
                payment.StripePaymentIntentId!,
                payment.StripeAccountId,
                amountToTransfer,
                releaseDTO.Reason ?? "Service completed successfully");

            if (!transferResult.IsSuccess)
            {
                logger.LogError("❌ Stripe transfer failed: {Error}", transferResult.Error?.Message);
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Transfer to specialist failed: {transferResult.Error?.Message}",
                    ErrorCodes.TechnicalError));
            }

            // ✅ Step 4: Update payment record
            await UpdatePaymentAsReleased(payment, amountToTransfer, transferResult.Result!, cancellationToken);

            logger.LogInformation("✅ Payment released successfully!");
            logger.LogInformation("💰 Specialist received: {TransferAmount} RON", amountToTransfer);
            logger.LogInformation("💼 Platform revenue: {PlatformRevenue} RON", payment.ProtectionFee);
            logger.LogInformation("🧾 Transfer ID: {TransferId}", transferResult.Result);

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error releasing payment: {PaymentId}", releaseDTO.PaymentId);
            return ServiceResponse.CreateErrorResponse(new(
                HttpStatusCode.InternalServerError,
                $"Payment release failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    /// <summary>
    /// Refund payment to client - works for both escrowed and released payments
    /// </summary>
    public async Task<ServiceResponse> RefundPayment(
        PaymentRefundDTO refundDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("💸 Processing refund for payment: {PaymentId}", refundDTO.PaymentId);

            // ✅ Step 1: Get and validate payment
            var payment = await GetPaymentById(refundDTO.PaymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (!payment.CanBeRefunded())
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    "Payment cannot be refunded",
                    ErrorCodes.CannotUpdate));
            }

            // ✅ Step 2: Calculate refund amount
            var maxRefundable = payment.GetMaxRefundableAmount();
            var refundAmount = refundDTO.Amount ?? maxRefundable;

            if (refundAmount > maxRefundable || refundAmount <= 0)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Invalid refund amount. Maximum refundable: {maxRefundable} RON",
                    ErrorCodes.CannotUpdate));
            }

            // ✅ Step 3: Process Stripe refund
            var refundResult = await stripeAccountService.RefundPayment(
                payment.StripePaymentIntentId!,
                refundAmount,
                refundDTO.Reason ?? "Service refund requested");

            if (!refundResult.IsSuccess)
            {
                logger.LogError("❌ Stripe refund failed: {Error}", refundResult.Error?.Message);
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Refund processing failed: {refundResult.Error?.Message}",
                    ErrorCodes.TechnicalError));
            }

            // ✅ Step 4: Update payment record
            await UpdatePaymentAsRefunded(payment, refundAmount, refundResult.Result!, cancellationToken);

            logger.LogInformation("✅ Refund processed successfully: {RefundId}", refundResult.Result);
            logger.LogInformation("💰 Refund amount: {RefundAmount} RON", refundAmount);

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error processing refund: {PaymentId}", refundDTO.PaymentId);
            return ServiceResponse.CreateErrorResponse(new(
                HttpStatusCode.InternalServerError,
                $"Refund processing failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    #endregion

    #region Payment Status & History

    /// <summary>
    /// Get detailed payment status including escrow information
    /// </summary>
    public async Task<ServiceResponse<PaymentStatusResponseDTO>> GetPaymentStatus(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await GetPaymentById(paymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse<PaymentStatusResponseDTO>(new(
                    HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            var status = new PaymentStatusResponseDTO
            {
                PaymentId = payment.Id,
                Status = payment.Status.GetStatusMessage(),
                IsEscrowed = payment.IsInEscrow(),
                CanBeReleased = payment.CanBeReleased(),
                CanBeRefunded = payment.CanBeRefunded(),
                AmountBreakdown = new PaymentAmountBreakdown
                {
                    ServiceAmount = payment.ServiceAmount,
                    ProtectionFee = payment.ProtectionFee,
                    TotalAmount = payment.TotalAmount,
                    TransferredAmount = payment.TransferredAmount,
                    RefundedAmount = payment.RefundedAmount,
                    PendingAmount = payment.GetEscrowedAmount(),
                    PlatformRevenue = payment.FeeCollected ? payment.ProtectionFee : 0
                },
                ProtectionFeeDetails = PaymentHelpers.GetProtectionFeeDetails(payment)
            };

            return ServiceResponse.CreateSuccessResponse(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error getting payment status: {PaymentId}", paymentId);
            return ServiceResponse.CreateErrorResponse<PaymentStatusResponseDTO>(new(
                HttpStatusCode.InternalServerError,
                $"Failed to get payment status: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    /// <summary>
    /// Get paginated payment history for user
    /// </summary>
    public async Task<ServiceResponse<PagedResponse<PaymentHistoryDTO>>> GetPaymentHistory(
        Guid userId,
        PaginationSearchQueryParams pagination,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await repository.PageAsync(
                pagination,
                new PaymentHistoryProjectionSpec(userId, pagination.Search),
                cancellationToken);

            return ServiceResponse.CreateSuccessResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error getting payment history for user: {UserId}", userId);
            return ServiceResponse.CreateErrorResponse<PagedResponse<PaymentHistoryDTO>>(new(
                HttpStatusCode.InternalServerError,
                $"Failed to retrieve payment history: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    /// <summary>
    /// Get detailed payment information
    /// </summary>
    public async Task<ServiceResponse<PaymentDetailsDTO>> GetPaymentDetails(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await GetPaymentById(paymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse<PaymentDetailsDTO>(new(
                    HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            var reply = await repository.GetAsync(new ReplySpec(payment.ReplyId), cancellationToken);
            if (reply == null)
            {
                return ServiceResponse.CreateErrorResponse<PaymentDetailsDTO>(new(
                    HttpStatusCode.NotFound,
                    "Associated reply not found",
                    ErrorCodes.EntityNotFound));
            }

            var paymentDetails = PaymentHelpers.ToPaymentDetailsDTO(payment, reply);

            return ServiceResponse.CreateSuccessResponse(paymentDetails);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error getting payment details: {PaymentId}", paymentId);
            return ServiceResponse.CreateErrorResponse<PaymentDetailsDTO>(new(
                HttpStatusCode.InternalServerError,
                $"Failed to retrieve payment details: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    #endregion

    #region Administrative Functions

    /// <summary>
    /// Cancel pending payment
    /// </summary>
    public async Task<ServiceResponse> CancelPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await GetPaymentById(paymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (!payment.CanBeCancelled())
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    $"Payment cannot be cancelled. Current status: {payment.Status.GetStatusMessage()}",
                    ErrorCodes.CannotUpdate));
            }

            // Cancel Stripe PaymentIntent if exists
            if (!string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                var service = new PaymentIntentService();
                await service.CancelAsync(payment.StripePaymentIntentId, cancellationToken: cancellationToken);
                logger.LogInformation("✅ Stripe PaymentIntent cancelled: {PaymentIntentId}", payment.StripePaymentIntentId);
            }

            // Update payment status
            payment.Status = PaymentStatusEnum.Cancelled;
            payment.CancelledAt = DateTime.UtcNow;

            await repository.UpdateAsync(payment, cancellationToken);

            logger.LogInformation("✅ Payment cancelled: {PaymentId}", paymentId);
            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error cancelling payment: {PaymentId}", paymentId);
            return ServiceResponse.CreateErrorResponse(new(
                HttpStatusCode.InternalServerError,
                $"Payment cancellation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    /// <summary>
    /// Generate platform revenue report
    /// </summary>
    public async Task<ServiceResponse<PaymentReportDTO>> GetRevenueReport(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ OPTION 1: Use projection spec for maximum efficiency (recommended for large datasets)
            var reportItems = await repository.ListAsync(
                new PaymentReportProjectionSpec(fromDate, toDate), 
                cancellationToken);

            logger.LogInformation("📊 Generating revenue report for {FromDate} to {ToDate} - Found {Count} payments",
                fromDate.ToShortDateString(), toDate.ToShortDateString(), reportItems.Count);

            var report = new PaymentReportDTO
            {
                Period = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                TotalServiceRevenue = reportItems.Sum(p => p.ServiceAmount),
                TotalProtectionFees = reportItems.Sum(p => p.ProtectionFee),
                TotalPlatformRevenue = reportItems.Where(p => p.FeeCollected).Sum(p => p.ProtectionFee),
                TotalTransactions = reportItems.Count,
                CompletedServices = reportItems.Count(p => p.Status == PaymentStatusEnum.Released),
                RefundedServices = reportItems.Count(p => p.Status == PaymentStatusEnum.Refunded),
                EscrowedPayments = reportItems.Count(p => p.IsEscrowed),
                RefundRate = reportItems.Count > 0 ?
                    (decimal)reportItems.Count(p => p.Status == PaymentStatusEnum.Refunded) / reportItems.Count * 100 : 0,
                AverageServiceValue = reportItems.Count > 0 ? reportItems.Average(p => p.ServiceAmount) : 0,
                AverageProtectionFee = reportItems.Count > 0 ? reportItems.Average(p => p.ProtectionFee) : 0,
                TotalEscrowedAmount = reportItems.Where(p => p.IsEscrowed)
                    .Sum(p => p.TotalAmount - p.TransferredAmount - p.RefundedAmount)
            };

            logger.LogInformation("💰 Report generated - Platform Revenue: {Revenue} RON, Transactions: {Count}",
                report.TotalPlatformRevenue, report.TotalTransactions);

            return ServiceResponse.CreateSuccessResponse(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error generating revenue report");
            return ServiceResponse.CreateErrorResponse<PaymentReportDTO>(new(
                HttpStatusCode.InternalServerError,
                $"Report generation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    #endregion

    #region Stripe Webhooks

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    public async Task<ServiceResponse> HandleStripeWebhook(
        string eventJson,
        string signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Event stripeEvent;
            try
            {
                // For development, parse without signature verification
                // In production, you should verify the signature
                stripeEvent = EventUtility.ParseEvent(eventJson);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "❌ Webhook parsing failed");
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.BadRequest,
                    "Invalid webhook event",
                    ErrorCodes.TechnicalError));
            }

            logger.LogInformation("🎣 Received Stripe webhook: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case EventTypes.PaymentIntentSucceeded:
                    await HandlePaymentIntentSucceeded(stripeEvent, cancellationToken);
                    break;

                case EventTypes.PaymentIntentPaymentFailed:
                    await HandlePaymentIntentFailed(stripeEvent, cancellationToken);
                    break;

                case EventTypes.PaymentIntentCanceled:
                    await HandlePaymentIntentCanceled(stripeEvent, cancellationToken);
                    break;

                case EventTypes.ChargeDisputeCreated:
                    await HandleChargeDispute(stripeEvent, cancellationToken);
                    break;

                default:
                    logger.LogInformation("ℹ️ Unhandled webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Webhook processing error");
            return ServiceResponse.CreateErrorResponse(new(
                HttpStatusCode.InternalServerError,
                $"Webhook processing failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    #endregion

    #region Legacy Support

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    [Obsolete("Use the new escrow payment methods instead")]
    public async Task<ServiceResponse<Payment>> AddPayment(
        PaymentAddDTO paymentDTO,
        CancellationToken cancellationToken = default)
    {
        var reply = await repository.GetAsync(new ReplySpec(paymentDTO.ReplyId), cancellationToken);
        if (reply == null)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                HttpStatusCode.NotFound,
                "Reply with this id not found!",
                ErrorCodes.EntityNotFound));
        }

        var payment = new Payment
        {
            ReplyId = paymentDTO.ReplyId,
            Reply = reply,
            ServiceAmount = paymentDTO.ServiceAmount,
            ProtectionFee = paymentDTO.ProtectionFee,
            TotalAmount = paymentDTO.ServiceAmount + paymentDTO.ProtectionFee,
            StripeAccountId = paymentDTO.StripeAccountId,
            Status = PaymentStatusEnum.Pending,
            Currency = "RON"
        };

        await repository.AddAsync(payment, cancellationToken);
        return ServiceResponse.CreateSuccessResponse(payment);
    }

    #endregion

    #region Private Helper Methods

    private ServiceResponse<PaymentIntentResponseDTO> ValidatePaymentAmounts(PaymentIntentCreateDTO createDTO)
    {
        var expectedTotal = createDTO.ServiceAmount + createDTO.ProtectionFee;
        if (Math.Abs(createDTO.TotalAmount - expectedTotal) > 0.01m)
        {
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                HttpStatusCode.BadRequest,
                $"Amount mismatch. Expected total: {expectedTotal}, Received: {createDTO.TotalAmount}",
                ErrorCodes.TechnicalError));
        }

        if (createDTO.ServiceAmount <= 0 || createDTO.ProtectionFee < 0 || createDTO.TotalAmount <= 0)
        {
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                HttpStatusCode.BadRequest,
                "Invalid amounts. Service amount must be positive, protection fee cannot be negative",
                ErrorCodes.TechnicalError));
        }

        return ServiceResponse.CreateSuccessResponse<PaymentIntentResponseDTO>(null!);
    }

    private async Task<(Reply? reply, User? specialist, string? accountId)> GetAndValidateReplyData(
        Guid replyId,
        CancellationToken cancellationToken)
    {
        var reply = await repository.GetAsync(new ReplySpec(replyId), cancellationToken);
        if (reply == null)
            return (null, null, null);

        var specialist = reply.Request.ReceiverUser;
        var accountId = specialist?.SpecialistProfile?.StripeAccountId;

        return (reply, specialist, accountId);
    }

    private async Task<PaymentIntentResponseDTO> CreateStripePaymentIntent(
        PaymentIntentCreateDTO createDTO,
        string specialistAccountId)
    {
        var stripeDto = new CreatePaymentIntentDTO
        {
            TotalAmount = createDTO.TotalAmount,
            ServiceAmount = createDTO.ServiceAmount,
            ProtectionFee = createDTO.ProtectionFee,
            SpecialistAccountId = specialistAccountId,
            Description = createDTO.Description ?? $"Service payment - Reply {createDTO.ReplyId}",
            Currency = createDTO.Currency,
            Metadata = createDTO.Metadata ?? new Dictionary<string, string>()
        };

        // Add additional metadata
        stripeDto.Metadata["reply_id"] = createDTO.ReplyId.ToString();

        return await stripeAccountService.CreatePaymentIntent(stripeDto);
    }

    private async Task<Payment> CreatePaymentRecord(
        PaymentIntentCreateDTO createDTO,
        Reply reply,
        string specialistAccountId,
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            ReplyId = createDTO.ReplyId,
            Reply = reply,
            ServiceAmount = createDTO.ServiceAmount,
            ProtectionFee = createDTO.ProtectionFee,
            TotalAmount = createDTO.TotalAmount,
            StripeAccountId = specialistAccountId,
            StripePaymentIntentId = paymentIntentId,
            Status = PaymentStatusEnum.Pending,
            Currency = createDTO.Currency,
            PlatformRevenue = createDTO.ProtectionFee,
            FeeCollected = false
        };

        if (createDTO.ProtectionFeeDetails != null)
        {
            PaymentHelpers.SetProtectionFeeDetails(payment, createDTO.ProtectionFeeDetails);
        }

        await repository.AddAsync(payment, cancellationToken);
        return payment;
    }

    private async Task<Payment?> GetPaymentByStripeId(string stripePaymentIntentId, CancellationToken cancellationToken)
    {
        return await repository.GetAsync(new PaymentSpec(stripePaymentIntentId), cancellationToken);
    }

    private async Task<Payment?> GetPaymentById(Guid paymentId, CancellationToken cancellationToken)
    {
        return await repository.GetAsync<Payment>(paymentId, cancellationToken);
    }

    private async Task<PaymentIntent?> VerifyStripePaymentStatus(string paymentIntentId, CancellationToken cancellationToken)
    {
        try
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "❌ Error verifying Stripe payment: {PaymentIntentId}", paymentIntentId);
            return null;
        }
    }

    private async Task UpdatePaymentToEscrowed(Payment payment, PaymentIntent stripePaymentIntent, CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatusEnum.Escrowed; // Money is now safely held in escrow
        payment.PaidAt = DateTime.UtcNow;
        payment.StripeChargeId = stripePaymentIntent.LatestChargeId;
        payment.FeeCollected = true; // Platform fee is now secured

        await repository.UpdateAsync(payment, cancellationToken);
    }

    private async Task UpdatePaymentAsReleased(Payment payment, decimal transferAmount, string transferId, CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatusEnum.Released;
        payment.EscrowReleasedAt = DateTime.UtcNow;
        payment.StripeTransferId = transferId;
        payment.TransferredAmount = transferAmount;
        // Platform revenue remains as the protection fee

        await repository.UpdateAsync(payment, cancellationToken);
    }

    private async Task UpdatePaymentAsRefunded(Payment payment, decimal refundAmount, string refundId, CancellationToken cancellationToken)
    {
        payment.RefundedAmount += refundAmount;
        payment.RefundedAt = DateTime.UtcNow;
        payment.StripeRefundId = refundId;

        // If fully refunded, update status
        if (payment.RefundedAmount >= payment.TotalAmount)
        {
            payment.Status = PaymentStatusEnum.Refunded;
        }

        await repository.UpdateAsync(payment, cancellationToken);
    }

    private async Task CreateServiceTaskFromPayment(Payment payment, CancellationToken cancellationToken)
    {
        // TODO: Implement service task creation
        // This would create the actual service booking after payment is confirmed
        logger.LogInformation("📋 Service task creation triggered for payment: {PaymentId}", payment.Id);
        
        // Example implementation:
        // var serviceTask = new ServiceTask
        // {
        //     PaymentId = payment.Id,
        //     ReplyId = payment.ReplyId,
        //     Status = ServiceTaskStatus.Scheduled,
        //     CreatedAt = DateTime.UtcNow
        // };
        // await _repository.AddAsync(serviceTask, cancellationToken);
    }

    #endregion

    #region Webhook Handlers

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                logger.LogWarning("❌ PaymentIntent data is null in webhook");
                return;
            }

            logger.LogInformation("✅ Payment succeeded: {PaymentIntentId} - Amount: {Amount} {Currency}",
                paymentIntent.Id, paymentIntent.Amount / 100m, paymentIntent.Currency?.ToUpper());

            var payment = await GetPaymentByStripeId(paymentIntent.Id, cancellationToken);
            if (payment == null)
            {
                logger.LogWarning("⚠️ Payment not found in database: {PaymentIntentId}", paymentIntent.Id);
                return;
            }

            // If payment isn't already confirmed, confirm it via webhook
            if (payment.Status == PaymentStatusEnum.Pending)
            {
                var confirmationDto = new PaymentConfirmationDTO
                {
                    PaymentIntentId = paymentIntent.Id,
                    ReplyId = payment.ReplyId,
                    ServiceAmount = payment.ServiceAmount,
                    ProtectionFee = payment.ProtectionFee,
                    TotalAmount = payment.TotalAmount,
                    PaymentMethod = $"Card ending in {paymentIntent.PaymentMethod?.Card?.Last4 ?? "****"}"
                };

                var result = await ConfirmPayment(confirmationDto, cancellationToken);
                if (result.IsSuccess)
                {
                    logger.LogInformation("✅ Payment confirmed via webhook: {PaymentIntentId}", paymentIntent.Id);
                }
                else
                {
                    logger.LogError("❌ Failed to confirm payment via webhook: {Error}", result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error handling PaymentIntentSucceeded webhook");
        }
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            logger.LogWarning("❌ Payment failed: {PaymentIntentId} - Reason: {Error}",
                paymentIntent.Id, paymentIntent.LastPaymentError?.Message);

            var payment = await GetPaymentByStripeId(paymentIntent.Id, cancellationToken);
            if (payment != null)
            {
                payment.Status = PaymentStatusEnum.Failed;
                payment.CancelledAt = DateTime.UtcNow;

                await repository.UpdateAsync(payment, cancellationToken);
                logger.LogInformation("💡 Updated payment {PaymentId} status to Failed", payment.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error handling PaymentIntentFailed webhook");
        }
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            logger.LogInformation("🚫 Payment canceled: {PaymentIntentId}", paymentIntent.Id);

            var payment = await GetPaymentByStripeId(paymentIntent.Id, cancellationToken);
            if (payment != null)
            {
                await CancelPayment(payment.Id, cancellationToken);
                logger.LogInformation("✅ Payment canceled in database: {PaymentId}", payment.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error handling PaymentIntentCanceled webhook");
        }
    }

    private async Task HandleChargeDispute(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            if (dispute == null) return;

            logger.LogWarning("⚠️ Charge dispute: {DisputeId} - Amount: {Amount} - Reason: {Reason}",
                dispute.Id, dispute.Amount / 100m, dispute.Reason);

            // Find the payment associated with this dispute
            var chargeId = dispute.ChargeId;
            if (!string.IsNullOrEmpty(chargeId))
            {
                // You might need to create a specification to find payment by charge ID
                // var payment = await _repository.GetAsync(new PaymentByChargeIdSpec(chargeId), cancellationToken);
                // if (payment != null)
                // {
                //     payment.Status = PaymentStatusEnum.Disputed;
                //     await _repository.UpdateAsync(payment, cancellationToken);
                // }
            }

            logger.LogInformation("🔍 Manual review required for dispute: {DisputeId}", dispute.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error handling ChargeDispute webhook");
        }
    }

    #endregion
    
    private ServiceResponse<PaymentIntentResponseDTO> ValidateOrCalculateAmounts(PaymentIntentCreateDTO createDTO)
    {
        // ✅ If amounts are not provided, calculate them using configuration
        if (createDTO.ProtectionFee == 0 || createDTO.TotalAmount == 0)
        {
            logger.LogInformation("📊 Calculating protection fee for service amount: {ServiceAmount}", createDTO.ServiceAmount);
        
            var feeBreakdown = feeConfigurationService.CalculateProtectionFee(createDTO.ServiceAmount);
        
            createDTO.ProtectionFee = feeBreakdown.FinalFee;
            createDTO.TotalAmount = createDTO.ServiceAmount + feeBreakdown.FinalFee;
        
            // Set fee details if not provided
            if (createDTO.ProtectionFeeDetails == null)
            {
                createDTO.ProtectionFeeDetails = new ProtectionFeeDetailsDTO
                {
                    BaseServiceAmount = feeBreakdown.BaseAmount,
                    FeeType = feeBreakdown.FeeType,
                    FeePercentage = feeBreakdown.PercentageRate,
                    FixedFeeAmount = feeBreakdown.FixedAmount,
                    MinimumFee = feeBreakdown.MinimumFee,
                    MaximumFee = feeBreakdown.MaximumFee,
                    CalculatedFee = feeBreakdown.FinalFee,
                    FeeJustification = feeBreakdown.Justification,
                    CalculatedAt = DateTime.UtcNow
                };
            }
        
            logger.LogInformation("💰 Fee calculated - Service: {ServiceAmount}, Fee: {ProtectionFee}, Total: {TotalAmount}",
                createDTO.ServiceAmount, createDTO.ProtectionFee, createDTO.TotalAmount);
        }

        // ✅ Now validate the amounts
        return ValidatePaymentAmounts(createDTO);
    }
}