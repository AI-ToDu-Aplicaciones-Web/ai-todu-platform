namespace AiToDu.Platform.Shared.Domain.Repositories;

/// <summary>
///     Base repository interface for all repositories
/// </summary>
public interface IBaseRepository<TEntity>
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TEntity?> FindByIdAsNoTrackingAsync(int id, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default);
}
