using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class PaymentSpec: Specification<Payment>
{
    public PaymentSpec(Guid id)
    {
        Query.Where(p => p.Id == id);
        // Query.Include(p => p.Reply)
        //     .ThenInclude(r => r.Request);
    }
    
    public PaymentSpec(string paymentIntentId)
    {
        Query.Where(p => p.StripePaymentIntentId == paymentIntentId);
    }
}