using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class ServiceTaskProjectionSpec: Specification<ServiceTask, ServiceTaskDTO>
{
    public ServiceTaskProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Select(e => new ServiceTaskDTO
        {
            Id = e.Id,
            UserId = e.UserId,
            SpecialistId = e.SpecialistId,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Description = e.Description,
            Address = e.Address,
            Price = e.Price,
            Status = e.Status,
            CompletedAt = e.CompletedAt,
            CancelledAt = e.CancelledAt,
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public ServiceTaskProjectionSpec(Guid id) : this()
    {
        Query.Where(e => e.Id == id);
    }

    public ServiceTaskProjectionSpec(string? search) : this(true)
    {
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;

        if (search == null)
            return;

        var searchExpr = $"%{search.Replace(" ", "%")}%";
        
        Query.Where(e =>
            EF.Functions.ILike(e.Description, searchExpr) ||
            EF.Functions.ILike(e.Address, searchExpr)
        );
    }
}