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
using Microsoft.Extensions.Configuration;

namespace ExpertEase.Infrastructure.Services;

public class PaymentService(IRepository<WebAppDatabaseContext> repository,
    IConfiguration configuration): IPaymentService
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
    
    public async Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentIntent(
        PaymentIntentCreateDTO createDTO, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify reply exists
            var reply = await repository.GetAsync(new ReplySpec(createDTO.ReplyId), cancellationToken);
            if (reply == null)
            {
                return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Reply not found",
                    ErrorCodes.EntityNotFound));
            }

            // Create Stripe PaymentIntent
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(createDTO.Amount * 100), // Convert to smallest currency unit (bani)
                Currency = createDTO.Currency.ToLower(),
                Description = createDTO.Description,
                Metadata = createDTO.Metadata ?? new Dictionary<string, string>(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            // Create payment record
            var payment = new Payment
            {
                ReplyId = createDTO.ReplyId,
                Reply = reply,
                Amount = createDTO.Amount,
                StripeAccountId = paymentIntent.Id, // Use PaymentIntent ID as account ID for now
                StripePaymentIntentId = paymentIntent.Id,
                Status = PaymentStatusEnum.Pending,
                Currency = createDTO.Currency
            };

            await repository.AddAsync(payment, cancellationToken);

            return ServiceResponse.CreateSuccessResponse(new PaymentIntentResponseDTO
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id
            });
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe error: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Payment intent creation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    // ✅ SIMPLIFIED: Returns just success/failure
    public async Task<ServiceResponse> ConfirmPayment(
        PaymentConfirmationDTO confirmationDTO, 
        CancellationToken cancellationToken = default)
    {
        try
        {
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
                    "Payment has not succeeded",
                    ErrorCodes.TechnicalError));
            }

            // Update payment status
            payment.Status = PaymentStatusEnum.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.StripeChargeId = paymentIntent.LatestChargeId;

            await repository.UpdateAsync(payment, cancellationToken);

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

    // ✅ SIMPLIFIED: Returns just success/failure
    public async Task<ServiceResponse> RefundPayment(
        PaymentRefundDTO refundDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
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
                Reason = refundDTO.Reason ?? "requested_by_customer"
            };

            var refund = await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

            // Update payment status
            payment.Status = PaymentStatusEnum.Refunded;
            payment.CancelledAt = DateTime.UtcNow;

            await repository.UpdateAsync(payment, cancellationToken);

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

    // ✅ SIMPLIFIED: Returns just success/failure
    public async Task<ServiceResponse> CancelPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
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
            // Get the webhook secret from configuration
            var endpointSecret = configuration["Stripe:WebhookSecret"];
            
            if (string.IsNullOrEmpty(endpointSecret))
            {
                Console.WriteLine("⚠️  Webhook secret not configured - skipping signature verification");
            }

            Event stripeEvent;
            
            if (!string.IsNullOrEmpty(endpointSecret) && !string.IsNullOrEmpty(signature))
            {
                // Verify webhook signature for security
                try
                {
                    stripeEvent = EventUtility.ConstructEvent(eventJson, signature, endpointSecret);
                }
                catch (StripeException ex)
                {
                    Console.WriteLine($"❌ Webhook signature verification failed: {ex.Message}");
                    return ServiceResponse.CreateErrorResponse(new(
                        System.Net.HttpStatusCode.BadRequest,
                        "Invalid signature",
                        ErrorCodes.TechnicalError));
                }
            }
            else
            {
                // Parse event without signature verification (development only)
                stripeEvent = EventUtility.ParseEvent(eventJson);
            }

            Console.WriteLine($"🎣 Received Stripe webhook: {stripeEvent.Type}");

            // Handle different webhook events
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

    /// <summary>
    /// Handle successful payment intent
    /// </summary>
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

            // Find the payment in our database
            var paymentResponse = await repository.GetAsync(new PaymentSpec(paymentIntent.Id), cancellationToken);
            if (paymentResponse == null)
            {
                Console.WriteLine($"⚠️  Payment not found in database: {paymentIntent.Id}");
                return;
            }

            // Update payment status if not already completed
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

    /// <summary>
    /// Handle failed payment intent
    /// </summary>
    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            Console.WriteLine($"❌ Payment failed: {paymentIntent.Id} - Reason: {paymentIntent.LastPaymentError?.Message}");

            // Find payment in database
            var paymentResponse = await repository.GetAsync(new PaymentSpec(paymentIntent.Id), cancellationToken);
            if (paymentResponse != null)
            {
                // Update payment status to failed
                paymentResponse.Status = PaymentStatusEnum.Failed; // You'll need to add this to your enum
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

    /// <summary>
    /// Handle canceled payment intent
    /// </summary>
    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            Console.WriteLine($"🚫 Payment canceled: {paymentIntent.Id}");

            // Find payment in database
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

    /// <summary>
    /// Handle charge disputes
    /// </summary>
    private async Task HandleChargeDispute(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            if (dispute == null) return;

            Console.WriteLine($"⚠️  Charge dispute: {dispute.Id} - Amount: {dispute.Amount / 100m} - Reason: {dispute.Reason}");

            // Log dispute for manual review
            // You might want to create a dispute tracking system or notification
            Console.WriteLine($"🔍 Manual review required for dispute: {dispute.Id}");
            
            // TODO: Consider adding dispute tracking to database
            // TODO: Consider sending notification to admins
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error handling ChargeDispute: {ex.Message}");
        }
    }
}