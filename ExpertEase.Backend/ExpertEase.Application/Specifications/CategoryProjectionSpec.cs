using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class CategoryProjectionSpec : Specification<Category, CategoryDTO>
{
    public CategoryProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Select(e => new CategoryDTO
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
        });
        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public CategoryProjectionSpec(Guid id) : this() => Query.Where(e => e.Id == id);

    public CategoryProjectionSpec(string? search) : this(true)
    {
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;

        if (search == null)
            return;

        var searchExpr = $"%{search.Replace(" ", "%")}%";

        Query.Where(e =>
            EF.Functions.ILike(e.Name, searchExpr)
        );
    }
}