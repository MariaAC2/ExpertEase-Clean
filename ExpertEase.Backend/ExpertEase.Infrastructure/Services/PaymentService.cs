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

namespace ExpertEase.Infrastructure.Services;

public class PaymentService(
    IRepository<WebAppDatabaseContext> repository,
    IStripeAccountService stripeAccountService) : IPaymentService // ✅ Only inject StripeAccountService, remove IConfiguration
{
    public async Task<ServiceResponse<Payment>> AddPayment(PaymentAddDTO paymentDTO, CancellationToken cancellationToken = default)
    {
        var reply = await repository.GetAsync(new ReplySpec(paymentDTO.ReplyId), cancellationToken);
        if (reply == null)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.NotFound, 
                "Reply with this id not found!", 
                ErrorCodes.EntityNotFound));
        }

        var payment = new Payment
        {
            ReplyId = paymentDTO.ReplyId,
            Reply = reply,
            Amount = paymentDTO.Amount,
            StripeAccountId = paymentDTO.StripeAccountId,
            Status = PaymentStatusEnum.Pending,
            Currency = "RON"
        };
        
        await repository.AddAsync(payment, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse(payment);
    }
    
    // ✅ CLEAN: Only use StripeAccountService
public async Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentIntent(
    PaymentIntentCreateDTO createDTO, 
    CancellationToken cancellationToken = default)
{
    try
    {
        Console.WriteLine($"🚀 Creating payment intent for ReplyId: {createDTO.ReplyId}, Amount: {createDTO.Amount}");

        // Verify reply exists and get specialist info
        var reply = await repository.GetAsync(new ReplySpec(createDTO.ReplyId), cancellationToken);
        if (reply == null)
        {
            Console.WriteLine($"❌ Reply not found: {createDTO.ReplyId}");
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                System.Net.HttpStatusCode.NotFound,
                "Reply not found",
                ErrorCodes.EntityNotFound));
        }

        Console.WriteLine($"✅ Reply found: {reply.Id}");

        // Get specialist's Stripe account ID
        var specialist = reply.Request.ReceiverUser;
        Console.WriteLine($"👤 Specialist: {specialist?.FullName ?? "Unknown"}");
        
        var specialistStripeAccountId = specialist?.SpecialistProfile?.StripeAccountId;
        Console.WriteLine($"🏦 Specialist Stripe Account ID: {specialistStripeAccountId ?? "NULL"}");
        
        if (string.IsNullOrEmpty(specialistStripeAccountId))
        {
            Console.WriteLine($"❌ Specialist {specialist?.FullName} doesn't have a Stripe account");
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                System.Net.HttpStatusCode.BadRequest,
                "Specialist doesn't have a connected Stripe account. They need to complete onboarding first.",
                ErrorCodes.TechnicalError));
        }

        // ✅ ENHANCED: Add validation before calling Stripe
        if (createDTO.Amount <= 0)
        {
            Console.WriteLine($"❌ Invalid amount: {createDTO.Amount}");
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                System.Net.HttpStatusCode.BadRequest,
                "Payment amount must be greater than 0",
                ErrorCodes.TechnicalError));
        }

        Console.WriteLine($"💳 Calling StripeAccountService.CreatePaymentIntent with:");
        Console.WriteLine($"   - Amount: {createDTO.Amount}");
        Console.WriteLine($"   - Specialist Account: {specialistStripeAccountId}");

        // ✅ USE StripeAccountService - it handles all Stripe configuration
        var clientSecret = await stripeAccountService.CreatePaymentIntent(
            createDTO.Amount, 
            specialistStripeAccountId);

        Console.WriteLine($"✅ Stripe payment intent created, client secret: {clientSecret?.Substring(0, 20) ?? "NULL"}...");

        // ✅ ENHANCED: Validate client secret
        if (string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine($"❌ StripeAccountService returned empty client secret");
            throw new Exception("StripeAccountService returned empty client secret");
        }

        // Extract PaymentIntent ID from client secret
        var paymentIntentId = ExtractPaymentIntentIdFromClientSecret(clientSecret);
        Console.WriteLine($"📝 Extracted PaymentIntent ID: {paymentIntentId}");

        // ✅ ENHANCED: Validate extracted ID
        if (string.IsNullOrEmpty(paymentIntentId))
        {
            Console.WriteLine($"❌ Failed to extract PaymentIntent ID from client secret");
            throw new Exception("Failed to extract PaymentIntent ID from client secret");
        }

        // Create payment record
        var payment = new Payment
        {
            ReplyId = createDTO.ReplyId,
            Reply = reply,
            Amount = createDTO.Amount,
            StripeAccountId = specialistStripeAccountId,
            StripePaymentIntentId = paymentIntentId,
            Status = PaymentStatusEnum.Pending,
            Currency = createDTO.Currency
        };

        Console.WriteLine($"💾 Saving payment record to database...");
        await repository.AddAsync(payment, cancellationToken);
        Console.WriteLine($"✅ Payment record saved with ID: {payment.Id}");

        return ServiceResponse.CreateSuccessResponse(new PaymentIntentResponseDTO
        {
            ClientSecret = clientSecret,
            PaymentIntentId = paymentIntentId
        });
    }
    catch (StripeException ex)
    {
        Console.WriteLine($"❌ Stripe error: {ex.Message}");
        Console.WriteLine($"❌ Stripe error code: {ex.StripeError?.Code}");
        Console.WriteLine($"❌ Stripe error type: {ex.StripeError?.Type}");
        
        return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
            System.Net.HttpStatusCode.BadRequest,
            $"Stripe error: {ex.Message} (Code: {ex.StripeError?.Code})",
            ErrorCodes.TechnicalError));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ General error in CreatePaymentIntent: {ex.Message}");
        Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        
        return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
            System.Net.HttpStatusCode.InternalServerError,
            $"Payment intent creation failed: {ex.Message}",
            ErrorCodes.TechnicalError));
    }
}

    // ✅ HELPER: Extract PaymentIntent ID from client secret
    private string ExtractPaymentIntentIdFromClientSecret(string clientSecret)
    {
        // Client secret format: "pi_1234567890_secret_abcdef"
        // We want just: "pi_1234567890"
        return clientSecret.Split('_').Take(2).Aggregate((a, b) => $"{a}_{b}");
    }

    public async Task<ServiceResponse> ConfirmPayment(
        PaymentConfirmationDTO confirmationDTO, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ NOTE: StripeAccountService already configures Stripe API key in constructor
            // We can directly use Stripe services

            // Get payment by PaymentIntent ID
            var payment = await repository.GetAsync(new PaymentSpec(confirmationDTO.PaymentIntentId), cancellationToken);

            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            // Verify payment with Stripe
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(confirmationDTO.PaymentIntentId, cancellationToken: cancellationToken);

            if (paymentIntent.Status != "succeeded")
            {
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.BadRequest,
                    $"Payment has not succeeded. Current status: {paymentIntent.Status}",
                    ErrorCodes.TechnicalError));
            }

            // Update payment status
            payment.Status = PaymentStatusEnum.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.StripeChargeId = paymentIntent.LatestChargeId;

            await repository.UpdateAsync(payment, cancellationToken);

            Console.WriteLine($"✅ Payment confirmed: {confirmationDTO.PaymentIntentId}");
            Console.WriteLine($"💰 Amount: {paymentIntent.Amount / 100m} {paymentIntent.Currency.ToUpper()}");
            Console.WriteLine($"👤 Specialist Account: {payment.StripeAccountId}");

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe verification failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Payment confirmation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    public async Task<ServiceResponse<PagedResponse<PaymentHistoryDTO>>> GetPaymentHistory(
        Guid userId,
        PaginationSearchQueryParams pagination,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await repository.PageAsync(pagination, new PaymentHistoryProjectionSpec(userId, pagination.Search), cancellationToken);
            return ServiceResponse.CreateSuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<PagedResponse<PaymentHistoryDTO>>(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Failed to retrieve payment history: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    public async Task<ServiceResponse<PaymentDetailsDTO>> GetPaymentDetails(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await repository.GetAsync(new PaymentProjectionSpec(paymentId), cancellationToken);

            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse<PaymentDetailsDTO>(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }
            
            var reply = await repository.GetAsync(new ReplySpec(payment.ReplyId), cancellationToken);

            var paymentDetails = new PaymentDetailsDTO
            {
                Id = payment.Id,
                ReplyId = payment.ReplyId,
                Amount = payment.Amount,
                Currency = payment.Currency ?? "RON",
                Status = payment.Status,
                PaidAt = payment.PaidAt,
                CreatedAt = payment.CreatedAt,
                StripePaymentIntentId = payment.StripePaymentIntentId,
                ServiceDescription = reply.Request.Description,
                ServiceAddress = reply.Request.Address,
                ServiceStartDate = reply.StartDate,
                ServiceEndDate = reply.EndDate,
                SpecialistName = reply.Request.ReceiverUser.FullName,
                ClientName = reply.Request.SenderUser.FullName,
            };

            return ServiceResponse.CreateSuccessResponse(paymentDetails);
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<PaymentDetailsDTO>(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Failed to retrieve payment details: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    public async Task<ServiceResponse> RefundPayment(
        PaymentRefundDTO refundDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // StripeAccountService already configured Stripe API key
            var payment = await repository.GetAsync<Payment>(refundDTO.PaymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (payment.Status != PaymentStatusEnum.Completed)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.BadRequest,
                    "Only completed payments can be refunded",
                    ErrorCodes.CannotUpdate));
            }

            // Create Stripe refund
            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = payment.StripePaymentIntentId,
                Amount = refundDTO.Amount.HasValue ? (long)(refundDTO.Amount.Value * 100) : null,
                Reason = refundDTO.Reason ?? "requested_by_customer",
                ReverseTransfer = true // Important: This reverses the transfer to specialist
            };

            var refund = await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

            // Update payment status
            payment.Status = PaymentStatusEnum.Refunded;
            payment.CancelledAt = DateTime.UtcNow;

            await repository.UpdateAsync(payment, cancellationToken);

            Console.WriteLine($"✅ Refund processed: {refund.Id} for payment {payment.StripePaymentIntentId}");

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe refund failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Refund processing failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    public async Task<ServiceResponse> CancelPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // StripeAccountService already configured Stripe API key
            var payment = await repository.GetAsync<Payment>(paymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (payment.Status != PaymentStatusEnum.Pending)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.BadRequest,
                    "Only pending payments can be cancelled",
                    ErrorCodes.CannotUpdate));
            }

            // Cancel Stripe PaymentIntent
            if (!string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                var service = new PaymentIntentService();
                await service.CancelAsync(payment.StripePaymentIntentId, cancellationToken: cancellationToken);
                Console.WriteLine($"✅ Stripe PaymentIntent cancelled: {payment.StripePaymentIntentId}");
            }

            // Update payment status
            payment.Status = PaymentStatusEnum.Cancelled;
            payment.CancelledAt = DateTime.UtcNow;

            await repository.UpdateAsync(payment, cancellationToken);

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe cancellation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Payment cancellation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    public async Task<ServiceResponse> HandleStripeWebhook(string eventJson, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            // StripeAccountService already configured the API key
            // For webhooks, we might need the webhook secret though
            // You'll need to add WebhookSecret to StripeSettings and expose it through IStripeAccountService
            
            Event stripeEvent;
            try
            {
                // For now, parse without signature verification (development only)
                stripeEvent = EventUtility.ParseEvent(eventJson);
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"❌ Webhook parsing failed: {ex.Message}");
                return ServiceResponse.CreateErrorResponse(new(
                    System.Net.HttpStatusCode.BadRequest,
                    "Invalid webhook event",
                    ErrorCodes.TechnicalError));
            }

            Console.WriteLine($"🎣 Received Stripe webhook: {stripeEvent.Type}");

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
                    Console.WriteLine($"ℹ️  Unhandled webhook event type: {stripeEvent.Type}");
                    break;
            }

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Webhook processing error: {ex.Message}");
            return ServiceResponse.CreateErrorResponse(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Webhook processing failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    // Webhook handlers - same as before
    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                Console.WriteLine("❌ PaymentIntent data is null");
                return;
            }

            Console.WriteLine($"✅ Payment succeeded: {paymentIntent.Id} - Amount: {paymentIntent.Amount / 100m} {paymentIntent.Currency.ToUpper()}");

            var paymentResponse = await repository.GetAsync(new PaymentSpec(paymentIntent.Id), cancellationToken);
            if (paymentResponse == null)
            {
                Console.WriteLine($"⚠️  Payment not found in database: {paymentIntent.Id}");
                return;
            }

            if (paymentResponse.Status != PaymentStatusEnum.Completed)
            {
                var confirmationDto = new PaymentConfirmationDTO
                {
                    PaymentIntentId = paymentIntent.Id,
                    ReplyId = paymentResponse.ReplyId
                };

                var result = await ConfirmPayment(confirmationDto, cancellationToken);
                
                if (result.IsOk)
                {
                    Console.WriteLine($"✅ Payment confirmed via webhook: {paymentIntent.Id}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to confirm payment via webhook: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error handling PaymentIntentSucceeded: {ex.Message}");
        }
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            Console.WriteLine($"❌ Payment failed: {paymentIntent.Id} - Reason: {paymentIntent.LastPaymentError?.Message}");

            var paymentResponse = await repository.GetAsync(new PaymentSpec(paymentIntent.Id), cancellationToken);
            if (paymentResponse != null)
            {
                paymentResponse.Status = PaymentStatusEnum.Failed;
                paymentResponse.CancelledAt = DateTime.UtcNow;
                
                await repository.UpdateAsync(paymentResponse, cancellationToken);
                Console.WriteLine($"💡 Updated payment {paymentResponse.Id} status to Failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error handling PaymentIntentFailed: {ex.Message}");
        }
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            Console.WriteLine($"🚫 Payment canceled: {paymentIntent.Id}");

            var paymentResponse = await repository.GetAsync(new PaymentSpec(paymentIntent.Id), cancellationToken);
            if (paymentResponse != null)
            {
                await CancelPayment(paymentResponse.Id, cancellationToken);
                Console.WriteLine($"✅ Payment canceled in database: {paymentResponse.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error handling PaymentIntentCanceled: {ex.Message}");
        }
    }

    private async Task HandleChargeDispute(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            if (dispute == null) return;

            Console.WriteLine($"⚠️  Charge dispute: {dispute.Id} - Amount: {dispute.Amount / 100m} - Reason: {dispute.Reason}");
            Console.WriteLine($"🔍 Manual review required for dispute: {dispute.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error handling ChargeDispute: {ex.Message}");
        }
    }
}