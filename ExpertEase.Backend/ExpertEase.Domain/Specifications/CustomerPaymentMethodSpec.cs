using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class CustomerPaymentMethodSpec: Specification<CustomerPaymentMethod>
{
    public CustomerPaymentMethodSpec(string stripeAccountId)
    {
        Query.Where(e => e.StripePaymentMethodId == stripeAccountId);
    }
    
    public CustomerPaymentMethodSpec(Guid customerId, bool? isDefault = null)
    {
        Query.Where(e => e.CustomerId == customerId);

        if (isDefault != null)
        {
            Query.Where(e => e.IsDefault == isDefault);
        }
    }
}