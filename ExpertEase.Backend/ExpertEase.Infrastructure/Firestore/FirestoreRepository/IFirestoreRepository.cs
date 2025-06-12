﻿using ExpertEase.Domain.Entities;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Firebase.FirestoreRepository;

public interface IFirestoreRepository
{
    Task<T?> GetAsync<T>(string collection, string id, CancellationToken cancellationToken = default) where T : FirestoreBaseEntityDTO;
    Task<T?> GetAsync<T>(string collection, Func<CollectionReference, Query> queryBuilder, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO;
    Task<List<T>> ListAsync<T>(string collection, CancellationToken cancellationToken = default) where T : FirestoreBaseEntityDTO;
    Task<List<T>> ListAsync<T>(string collection,
        Func<CollectionReference, Query> queryBuilder, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO;
    Task<List<TDto>> ListAsync<T, TDto>(string collection, Func<T, TDto> mapper, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO;
    Task<T> AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default) where T : FirestoreBaseEntityDTO;
    Task<T> UpdateAsync<T>(string collection, T entity, CancellationToken cancellationToken = default) where T : FirestoreBaseEntityDTO;
    Task DeleteAsync<T>(string collection, string id, CancellationToken cancellationToken = default) where T : FirestoreBaseEntityDTO;
}