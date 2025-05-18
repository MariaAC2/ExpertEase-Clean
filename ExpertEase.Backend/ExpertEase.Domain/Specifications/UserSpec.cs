using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

/// <summary>
/// This is a simple specification to filter the user entities from the database via the constructors.
/// The specification will extract the raw entities from the database without a projection.
/// Note that this is a sealed class, meaning it cannot be further derived.
/// </summary>
public sealed class UserSpec : Specification<User>
{
    public UserSpec(Guid id)
    {
        Query.Where(e => e.Id == id);

        Query.Include(e => e.Account);
        Query.Include(e => e.SpecialistProfile);
        Query.Include(e=> e.Requests)
            .ThenInclude(r => r.ReceiverUser);
    }

    public UserSpec(string email)
    {
        Query.Where(e => e.Email == email);
        Query.Include(e => e.Account);
        Query.Include(e => e.SpecialistProfile);
        Query.Include(e => e.Requests);
    }
}