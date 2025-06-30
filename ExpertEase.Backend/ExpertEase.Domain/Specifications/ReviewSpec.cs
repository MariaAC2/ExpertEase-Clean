using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public sealed class ReviewSpec : Specification<Review>
{
    public ReviewSpec(Guid id)
    {
        Query.Include(e=> e.SenderUser);
        Query.Include(e => e.ReceiverUser);
        Query.Where(e => e.Id == id);
    }
}

public sealed class ReviewByServiceTaskSpec : Specification<Review>
{
    public ReviewByServiceTaskSpec(Guid id)
    {
        Query.Include(e=> e.SenderUser);
        Query.Include(e => e.ReceiverUser);
        Query.Where(e => e.ServiceTaskId == id);
    }
}

public sealed class ReviewSearchSpec : Specification<Review>
{
    public ReviewSearchSpec(Review review)
    {
        Query.Where(e =>
            e.SenderUserId == review.SenderUserId &&
            e.ReceiverUserId == review.ReceiverUserId &&
            e.Content == review.Content &&
            e.Rating == review.Rating
        );
    }
}