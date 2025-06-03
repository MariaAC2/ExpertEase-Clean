using ExpertEase.Domain.Entities;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Repositories;

public class FirebaseRepository(FirestoreDb _firestoreDb) : IFirebaseRepository
{
    public async Task<T?> GetAsync<T>(string collection, Guid id, CancellationToken cancellationToken = default)
        where T : FirebaseBaseEntity
    {
        var doc = await _firestoreDb.Collection(collection).Document(id.ToString()).GetSnapshotAsync(cancellationToken);
        return doc.Exists ? doc.ConvertTo<T>() : null;
    }
    
    public async Task<List<T>> ListAsync<T>(
        string collection,
        CancellationToken cancellationToken = default
    ) where T : FirebaseBaseEntity
    {
        var collectionRef = _firestoreDb.Collection(collection);
        var snapshot = await collectionRef.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
    }
    
    public async Task<T> AddAsync<T>(string collection, T entity, CancellationToken cancellationToken = default)
        where T : FirebaseBaseEntity
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        await _firestoreDb.Collection(collection)
            .Document(entity.Id.ToString())
            .SetAsync(entity, cancellationToken: cancellationToken);

        return entity;
    }

    public async Task<T> UpdateAsync<T>(string collection, T entity, CancellationToken cancellationToken = default)
        where T : FirebaseBaseEntity
    {
        entity.UpdatedAt = DateTime.UtcNow;

        await _firestoreDb.Collection(collection)
            .Document(entity.Id.ToString())
            .SetAsync(entity, SetOptions.Overwrite, cancellationToken);

        return entity;
    }

    public async Task DeleteAsync<T>(string collection, Guid id, CancellationToken cancellationToken = default)
        where T : FirebaseBaseEntity
    {
        await _firestoreDb.Collection(collection)
            .Document(id.ToString())
            .DeleteAsync(cancellationToken: cancellationToken);
    }
}