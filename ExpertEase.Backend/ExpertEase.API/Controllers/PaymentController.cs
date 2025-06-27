using System.Net;
using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.DataTransferObjects.ProtectionFeeDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using ExpertEase.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DetailedProtectionFeeResponseDTO = ExpertEase.Application.DataTransferObjects.ProtectionFeeDTOs.DetailedProtectionFeeResponseDTO;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PaymentController(IUserService userService, IPaymentService paymentService) : AuthorizedController(userService)
{
    /// <summary>
    /// Creates escrow payment intent - money held until service completion
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse<PaymentIntentResponseDTO>>> CreatePaymentIntent(
        [FromBody] PaymentIntentCreateDTO createDTO)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.CreatePaymentIntent(createDTO))
            : CreateErrorMessageResult<PaymentIntentResponseDTO>(currentUser.Error);
    }
    
    /// <summary>
    /// Confirms payment after successful Stripe processing - money goes into escrow
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> ConfirmPayment(
        [FromBody] PaymentConfirmationDTO confirmationDTO)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.ConfirmPayment(confirmationDTO))
            : CreateErrorMessageResult(currentUser.Error);
    }
    
    /// <summary>
    /// ✅ NEW: Release escrowed money to specialist when service is completed
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> ReleasePayment(
        [FromBody] PaymentReleaseDTO releaseDTO)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult(currentUser.Error);

        // TODO: Add authorization logic - only allow:
        // - Admins
        // - Service clients (who paid)
        // - Platform automated processes
        
        return CreateRequestResponseFromServiceResponse(
            await paymentService.ReleasePayment(releaseDTO));
    }
    
    /// <summary>
    /// ✅ NEW: Get detailed payment status including escrow information
    /// </summary>
    [Authorize]
    [HttpGet("{paymentId:guid}/status")]
    public async Task<ActionResult<RequestResponse<PaymentStatusResponseDTO>>> GetPaymentStatus(
        [FromRoute] Guid paymentId)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult<PaymentStatusResponseDTO>(currentUser.Error);

        // TODO: Add authorization logic - only allow users related to the payment
        
        return CreateRequestResponseFromServiceResponse(
            await paymentService.GetPaymentStatus(paymentId));
    }
    
    /// <summary>
    /// Retrieves payment history for the authenticated user with escrow details
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<PaymentHistoryDTO>>>> GetPaymentHistory(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.GetPaymentHistory(currentUser.Result.Id, pagination))
            : CreateErrorMessageResult<PagedResponse<PaymentHistoryDTO>>(currentUser.Error);
    }
    
    /// <summary>
    /// Retrieves specific payment details with escrow information
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<PaymentDetailsDTO>>> GetPaymentDetails([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult<PaymentDetailsDTO>(currentUser.Error);

        // TODO: Add authorization logic - only allow users related to the payment
        
        return CreateRequestResponseFromServiceResponse(await paymentService.GetPaymentDetails(id));
    }
    
    /// <summary>
    /// ✅ UPDATED: Enhanced refund with escrow support - can refund escrowed or released payments
    /// </summary>
    [Authorize(Roles = "Admin")] // You might want to also allow service owners
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> RefundPayment(
        [FromBody] PaymentRefundDTO refundDTO)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.RefundPayment(refundDTO))
            : CreateErrorMessageResult(currentUser.Error);
    }
    
    /// <summary>
    /// Cancels a pending payment (before money is captured)
    /// </summary>
    [Authorize]
    [HttpPost("{paymentId:guid}")]
    public async Task<ActionResult<RequestResponse>> CancelPayment([FromRoute] Guid paymentId)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult(currentUser.Error);

        // TODO: Add authorization logic - only allow payment owner or admin
        
        return CreateRequestResponseFromServiceResponse(await paymentService.CancelPayment(paymentId));
    }
    
    /// <summary>
    /// ✅ NEW: Admin endpoint to get platform revenue report with escrow analytics
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PaymentReportDTO>>> GetRevenueReport(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult<PaymentReportDTO>(currentUser.Error);

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;
        
        return CreateRequestResponseFromServiceResponse(
            await paymentService.GetRevenueReport(from, to));
    }
    
    /// <summary>
    /// ✅ NEW: Calculate protection fee for a service amount (useful for frontend estimates)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse<CalculateProtectionFeeResponseDTO>>> CalculateProtectionFee(
        [FromBody] CalculateProtectionFeeRequestDTO request)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult<CalculateProtectionFeeResponseDTO>(currentUser.Error);

        try
        {
            // Validate input
            if (request.ServiceAmount <= 0)
            {
                return CreateErrorMessageResult<CalculateProtectionFeeResponseDTO>(new(
                        HttpStatusCode.BadRequest,
                        "Service amount must be greater than 0",
                        ErrorCodes.Invalid));
            }

            // This would use your IProtectionFeeConfigurationService
            // For now, using extension method directly
            var feeBreakdown = request.ServiceAmount.CalculateProtectionFee();
            
            var response = new CalculateProtectionFeeResponseDTO
            {
                ServiceAmount = request.ServiceAmount,
                ProtectionFee = feeBreakdown.FinalFee,
                TotalAmount = request.ServiceAmount + feeBreakdown.FinalFee,
                FeeJustification = feeBreakdown.Justification,
                FeeConfiguration = new ProtectionFeeConfigurationDTO
                {
                    FeeType = feeBreakdown.FeeType,
                    PercentageRate = feeBreakdown.PercentageRate,
                    FixedAmount = feeBreakdown.FixedAmount,
                    MinimumFee = feeBreakdown.MinimumFee,
                    MaximumFee = feeBreakdown.MaximumFee,
                    IsEnabled = true,
                    Description = "Client protection fee for service quality assurance",
                    LastUpdated = DateTime.UtcNow
                }
            };

            return CreateRequestResponseFromServiceResponse(
                ServiceResponse.CreateSuccessResponse(response));
        }
        catch (Exception ex)
        {
            return CreateErrorMessageResult<CalculateProtectionFeeResponseDTO>(new(
                    HttpStatusCode.InternalServerError,
                    $"Fee calculation failed: {ex.Message}",
                    ErrorCodes.TechnicalError));
        }
    }
    
    /// <summary>
    /// ✅ NEW: Get current protection fee configuration (for admin/frontend)
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<ProtectionFeeConfigurationDTO>>> GetProtectionFeeConfiguration()
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult<ProtectionFeeConfigurationDTO>(currentUser.Error);

        try
        {
            // This would use your IProtectionFeeConfigurationService
            // For now, returning hardcoded config
            var config = new ProtectionFeeConfigurationDTO
            {
                FeeType = "percentage",
                PercentageRate = 10.0m,
                FixedAmount = 25.0m,
                MinimumFee = 5.0m,
                MaximumFee = 100.0m,
                IsEnabled = true,
                Description = "Client protection fee for service quality assurance",
                LastUpdated = DateTime.UtcNow
            };

            return CreateRequestResponseFromServiceResponse(
                ServiceResponse.CreateSuccessResponse(config));
        }
        catch (Exception ex)
        {
            return CreateErrorMessageResult<ProtectionFeeConfigurationDTO>(new(
                    HttpStatusCode.InternalServerError,
                    $"Configuration retrieval failed: {ex.Message}",
                    ErrorCodes.TechnicalError));
        }
    }
    
    /// <summary>
    /// ✅ NEW: Get detailed protection fee breakdown (for transparency)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse<DetailedProtectionFeeResponseDTO>>> GetDetailedProtectionFeeBreakdown(
        [FromBody] CalculateProtectionFeeRequestDTO request)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult<DetailedProtectionFeeResponseDTO>(currentUser.Error);

        try
        {
            if (request.ServiceAmount <= 0)
            {
                return CreateErrorMessageResult<DetailedProtectionFeeResponseDTO>(new(
                        HttpStatusCode.BadRequest,
                        "Service amount must be greater than 0",
                        ErrorCodes.Invalid));
            }

            var feeBreakdown = request.ServiceAmount.CalculateProtectionFee();
            
            var response = new DetailedProtectionFeeResponseDTO
            {
                ServiceAmount = request.ServiceAmount,
                ProtectionFee = feeBreakdown.FinalFee,
                TotalAmount = request.ServiceAmount + feeBreakdown.FinalFee,
                Breakdown = new ProtectionFeeBreakdownDTO
                {
                    BaseServiceAmount = feeBreakdown.BaseAmount,
                    FeeType = feeBreakdown.FeeType,
                    PercentageRate = feeBreakdown.PercentageRate,
                    FixedAmount = feeBreakdown.FixedAmount,
                    MinimumFee = feeBreakdown.MinimumFee,
                    MaximumFee = feeBreakdown.MaximumFee,
                    CalculatedFeeBeforeLimits = feeBreakdown.CalculatedFee,
                    FinalFee = feeBreakdown.FinalFee,
                    Justification = feeBreakdown.Justification,
                    MinimumApplied = feeBreakdown.CalculatedFee < feeBreakdown.MinimumFee,
                    MaximumApplied = feeBreakdown.CalculatedFee > feeBreakdown.MaximumFee,
                    CalculatedAt = DateTime.UtcNow
                },
                Configuration = new ProtectionFeeConfigurationDTO
                {
                    FeeType = feeBreakdown.FeeType,
                    PercentageRate = feeBreakdown.PercentageRate,
                    FixedAmount = feeBreakdown.FixedAmount,
                    MinimumFee = feeBreakdown.MinimumFee,
                    MaximumFee = feeBreakdown.MaximumFee,
                    IsEnabled = true,
                    Description = "Client protection fee for service quality assurance",
                    LastUpdated = DateTime.UtcNow
                },
                Summary = $"You'll pay {feeBreakdown.FinalFee:F2} RON protection fee ({feeBreakdown.Justification}) on top of the {request.ServiceAmount:F2} RON service price."
            };

            return CreateRequestResponseFromServiceResponse(
                ServiceResponse.CreateSuccessResponse(response));
        }
        catch (Exception ex)
        {
            return CreateErrorMessageResult<DetailedProtectionFeeResponseDTO>(new(
                    HttpStatusCode.InternalServerError,
                    $"Detailed calculation failed: {ex.Message}",
                    ErrorCodes.TechnicalError));
        }
    }
    
    /// <summary>
    /// Webhook endpoint for Stripe payment events
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<RequestResponse>> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
    
        var result = await paymentService.HandleStripeWebhook(json, signature);
    
        return result.IsSuccess
            ? CreateRequestResponseFromServiceResponse(result)
            : CreateErrorMessageResult(result.Error);
    }
}