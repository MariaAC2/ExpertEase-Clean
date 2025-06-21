using Ardalis.Specification;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Infrastructure.Repositories;

/// <summary>
/// This interface provides the generic methods to work with the database context easier and use the specification design pattern.
/// </summary>
public interface IRepository<out TDb> where TDb : DbContext
{
    public TDb DbContext { get; }
    public Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<T?> GetAsync<T>(ISpecification<T> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<TOut?> GetAsync<T, TOut>(ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<int> GetCountAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<int> GetCountAsync<T>(ISpecification<T> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<int> GetCountAsync<T, TOut>(ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<List<T>> ListAsync<T>(ISpecification<T> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<List<T>> ListNoTrackingAsync<T>(ISpecification<T> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<List<TOut>> ListAsync<T, TOut>(ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<PagedResponse<T>> PageAsync<T>(PaginationQueryParams pagination, ISpecification<T> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<PagedResponse<TOut>> PageAsync<T, TOut>(PaginationQueryParams pagination, ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<T> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;
    public Task<TOut> AddAsync<T, TOut>(T entity, ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Adds multiple entities to the database.
    /// </summary>
    public Task<List<T>> AddRangeAsync<T>(List<T> entities, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Adds multiple entities to the database and projects them to another object type.
    /// </summary>
    public Task<List<TOut>> AddRangeAsync<T, TOut>(List<T> entities, ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Adds multiple entities to the database and gets the added entities as un-tracked objects.
    /// </summary>
    public Task AddRangeNoTrackingAsync<T>(List<T> entities, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Updates an entity to the database.
    /// </summary>
    public Task<T> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Updates an entity to the database and gets the projection via a specification.
    /// </summary>
    public Task<TOut> UpdateAsync<T, TOut>(T entity, ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Updates multiple entities to the database.
    /// </summary>
    public Task<List<T>> UpdateRangeAsync<T>(List<T> entities, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Updates multiple entities to the database and projects them to another object type.
    /// </summary>
    public Task<List<TOut>> UpdateRangeAsync<T, TOut>(List<T> entities, ISpecification<T, TOut> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Deletes an entity identified by the id.
    /// </summary>
    public Task<int> DeleteAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;
    /// <summary>
    /// Deletes entities that satisfy the specifications.
    /// </summary>
    public Task<int> DeleteAsync<T>(ISpecification<T> spec, CancellationToken cancellationToken = default) where T : BaseEntity;
}