using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public sealed class RequestSpec : Specification<Request>
{
    public RequestSpec(Guid id) => Query.Where(e => e.Id == id);
}

public sealed class RequestSearchSpec : Specification<Request>
{
    public RequestSearchSpec(Guid senderUserId, Guid receiverUserId) => 
        Query.Where(e => e.SenderUserId == senderUserId && e.ReceiverUserId == receiverUserId);
}

public sealed class RequestUserSpec : Specification<Request>
{
    public RequestUserSpec(Guid id, Guid userId)
    {
        Query.Include(e => e.SenderUser);
        Query.Where(e => e.SenderUserId == userId && e.Id == id);
    }
}

public sealed class RequestSpecialistSpec : Specification<Request>
{
    public RequestSpecialistSpec(Guid id, Guid userId) => 
        Query.Where(e => e.ReceiverUserId == userId && e.Id == id);
}