using Ardalis.Specification;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Specifications;

public sealed class RequestSpec : Specification<Request>
{
    public RequestSpec(Guid id)
    {
        Query.Where(e => e.Id == id);
        Query.Include(r => r.Replies);
    }
}

public sealed class RequestSearchSpec : Specification<Request>
{
    public RequestSearchSpec(Request request)
    {
        Query.Where(e =>
            e.SenderUserId == request.SenderUserId &&
            e.ReceiverUserId == request.ReceiverUserId &&
            e.RequestedStartDate == request.RequestedStartDate
        );
    }
}

public sealed class RequestUserSpec : Specification<Request>
{
    public RequestUserSpec(Guid userId)
    {
        Query.Where(e => e.SenderUserId == userId);
    }
}

public sealed class RequestSpecialistSpec : Specification<Request>
{
    public RequestSpecialistSpec(Guid id, Guid userId) => 
        Query.Where(e => e.ReceiverUserId == userId && e.Id == id);
}