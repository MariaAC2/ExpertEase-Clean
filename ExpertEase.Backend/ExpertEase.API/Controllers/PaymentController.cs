using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PaymentController(IUserService userService, IPaymentService paymentService) : AuthorizedController(userService)
{
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
    /// Confirms payment after successful Stripe processing and creates/updates service task
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse<ServiceTask>>> ConfirmPayment(
        [FromBody] PaymentConfirmationDTO confirmationDTO)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.ConfirmPayment(confirmationDTO))
            : CreateErrorMessageResult<ServiceTask>(currentUser.Error);
    }
    
    /// <summary>
    /// Retrieves payment history for the authenticated user
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<PaymentHistoryDTO>>>> GetPaymentHistory([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.GetPaymentHistory(currentUser.Result.Id, pagination))
            : CreateErrorMessageResult<PagedResponse<PaymentHistoryDTO>>(currentUser.Error);
    }
    
    /// <summary>
    /// Retrieves specific payment details
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<PaymentDetailsDTO>>> GetPaymentDetails([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.GetPaymentDetails(id))
            : CreateErrorMessageResult<PaymentDetailsDTO>(currentUser.Error);
    }
    
    /// <summary>
    /// Processes a refund for a completed payment (Admin only or service owner)
    /// </summary>
    [Authorize(Roles = "Admin")] // You might want to also allow service owners
    [HttpPost]
    public async Task<ActionResult<RequestResponse<Payment>>> RefundPayment(
        [FromBody] PaymentRefundDTO refundDTO)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.RefundPayment(refundDTO))
            : CreateErrorMessageResult<Payment>(currentUser.Error);
    }
    
    /// <summary>
    /// Cancels a pending payment
    /// </summary>
    [Authorize]
    [HttpPost("{paymentId:guid}")]
    public async Task<ActionResult<RequestResponse<Payment>>> CancelPayment([FromRoute] Guid paymentId)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await paymentService.CancelPayment(paymentId))
            : CreateErrorMessageResult<Payment>(currentUser.Error);
    }
    
    /// <summary>
    /// Webhook endpoint for Stripe payment events
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            // For now, just log the webhook - you can add proper handling later
            Console.WriteLine($"Received Stripe webhook: {json}");
            
            // TODO: Add proper webhook signature verification and event handling
            // This is just a placeholder to make the endpoint work
            
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook error: {ex.Message}");
            return BadRequest($"Webhook processing failed: {ex.Message}");
        }
    }
}