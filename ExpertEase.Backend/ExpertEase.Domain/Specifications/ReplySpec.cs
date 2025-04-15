using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public sealed class ReplySpec : Specification<Reply>
{
    public ReplySpec(Guid id) => Query.Where(rep => rep.Id == id);
}