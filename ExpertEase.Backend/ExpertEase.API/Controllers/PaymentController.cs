using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PaymentController(IUserService userService, IPaymentService paymentService) : AuthorizedController(userService)
{
    
}