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
using Stripe;

namespace ExpertEase.Infrastructure.Services;

public class PaymentService(IRepository<WebAppDatabaseContext> repository): IPaymentService
{
    public async Task<ServiceResponse<Payment>> AddPayment(PaymentAddDTO paymentDTO, CancellationToken cancellationToken = default)
    {
        var reply = await repository.GetAsync(new ReplySpec(paymentDTO.ReplyId), cancellationToken);
        if (reply == null)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.NotFound, 
                "Service task with this id not found!", 
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
            // Verify service task exists
            var serviceTask = await repository.GetAsync(new ReplySpec(createDTO.ReplyId), cancellationToken);
            if (serviceTask == null)
            {
                return ServiceResponse.CreateErrorResponse<PaymentIntentResponseDTO>(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Service task not found",
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
                Reply = serviceTask,
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

    public async Task<ServiceResponse<ServiceTask>> ConfirmPayment(
        PaymentConfirmationDTO confirmationDTO, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get payment by PaymentIntent ID
            var payment = await repository.GetAsync(new PaymentSpec(confirmationDTO.PaymentIntentId), cancellationToken);

            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse<ServiceTask>(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            // Verify payment with Stripe
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(confirmationDTO.PaymentIntentId, cancellationToken: cancellationToken);

            if (paymentIntent.Status != "succeeded")
            {
                return ServiceResponse.CreateErrorResponse<ServiceTask>(new(
                    System.Net.HttpStatusCode.BadRequest,
                    "Payment has not succeeded",
                    ErrorCodes.TechnicalError));
            }

            // Update payment status
            payment.Status = PaymentStatusEnum.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.StripeChargeId = paymentIntent.LatestChargeId;

            // Update service task status
            var serviceTask = await repository.GetAsync(new ServiceTaskSpec(payment.ReplyId), cancellationToken);
            if (serviceTask != null)
            {
                serviceTask.Status = JobStatusEnum.Confirmed;
                await repository.UpdateAsync(serviceTask, cancellationToken);
            }

            await repository.UpdateAsync(payment, cancellationToken);

            return ServiceResponse.CreateSuccessResponse(serviceTask!);
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse<ServiceTask>(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe verification failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<ServiceTask>(new(
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
            
            var serviceTask = await repository.GetAsync(new ServiceTaskDetailsProjectionSpec(payment.ReplyId), cancellationToken);

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
                ServiceDescription = serviceTask.Description,
                ServiceAddress = serviceTask.Address,
                ServiceStartDate = serviceTask.StartDate,
                ServiceEndDate = serviceTask.EndDate,
                SpecialistName = serviceTask.SpecialistName,
                ClientName = serviceTask.ClientName
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

    public async Task<ServiceResponse<Payment>> RefundPayment(
        PaymentRefundDTO refundDTO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await repository.GetAsync<Payment>(refundDTO.PaymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse<Payment>(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (payment.Status != PaymentStatusEnum.Completed)
            {
                return ServiceResponse.CreateErrorResponse<Payment>(new(
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

            return ServiceResponse.CreateSuccessResponse(payment);
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe refund failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Refund processing failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }

    public async Task<ServiceResponse<Payment>> CancelPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await repository.GetAsync<Payment>(paymentId, cancellationToken);
            if (payment == null)
            {
                return ServiceResponse.CreateErrorResponse<Payment>(new(
                    System.Net.HttpStatusCode.NotFound,
                    "Payment not found",
                    ErrorCodes.EntityNotFound));
            }

            if (payment.Status != PaymentStatusEnum.Pending)
            {
                return ServiceResponse.CreateErrorResponse<Payment>(new(
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

            return ServiceResponse.CreateSuccessResponse(payment);
        }
        catch (StripeException ex)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.BadRequest,
                $"Stripe cancellation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.InternalServerError,
                $"Payment cancellation failed: {ex.Message}",
                ErrorCodes.TechnicalError));
        }
    }
}