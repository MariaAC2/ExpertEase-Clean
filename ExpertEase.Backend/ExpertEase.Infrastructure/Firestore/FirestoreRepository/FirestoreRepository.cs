using ExpertEase.Domain.Entities;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Firebase.FirestoreRepository;

public class FirestoreRepository(FirestoreDb _firestoreDb) : IFirestoreRepository
{
    public async Task<T?> GetAsync<T>(string collection, string id, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO
    {
        var doc = await _firestoreDb.Collection(collection).Document(id.ToString()).GetSnapshotAsync(cancellationToken);
        return doc.Exists ? doc.ConvertTo<T>() : null;
    }
    
    public async Task<List<T>> ListAsync<T>(
        string collection,
        CancellationToken cancellationToken = default
    ) where T : FirestoreBaseEntityDTO
    {
        var collectionRef = _firestoreDb.Collection(collection);
        var snapshot = await collectionRef.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
    }
    
    public async Task<T?> GetAsync<T>(string collection, Func<CollectionReference, Query> queryBuilder, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO
    {
        var collectionRef = _firestoreDb.Collection(collection);
        var query = queryBuilder(collectionRef);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        var doc = snapshot.Documents.FirstOrDefault();
        if (doc == null)
            return null;

        var entity = doc.ConvertTo<T>();
        entity.Id = doc.Id;
        return entity;
    }
    
    public async Task<List<T>> ListAsync<T>(string collection, Func<CollectionReference, Query> queryBuilder, CancellationToken cancellationToken = default) where T : FirestoreBaseEntityDTO
    {
        var collectionRef = _firestoreDb.Collection(collection);
        var query = queryBuilder(collectionRef);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents
            .Select(doc => doc.ConvertTo<T>())
            .ToList();
    }
    
    public async Task<List<TDto>> ListAsync<T, TDto>(string collection, Func<T, TDto> mapper, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO
    {
        var entities = await ListAsync<T>(collection, cancellationToken);
        return entities.Select(mapper).ToList();
    }
    
    public async Task<T> AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO
    {
        entity.CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow);

        await _firestoreDb.Collection(collection)
            .Document(entity.Id.ToString())
            .SetAsync(entity, cancellationToken: cancellationToken);

        return entity;
    }

    public async Task<T> UpdateAsync<T>(string collection, T entity, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO
    {
        await _firestoreDb.Collection(collection)
            .Document(entity.Id.ToString())
            .SetAsync(entity, SetOptions.Overwrite, cancellationToken);

        return entity;
    }

    public async Task DeleteAsync<T>(string collection, string id, CancellationToken cancellationToken = default)
        where T : FirestoreBaseEntityDTO
    {
        await _firestoreDb.Collection(collection)
            .Document(id)
            .DeleteAsync(cancellationToken: cancellationToken);
    }
}