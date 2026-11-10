namespace AiToDu.Platform.Shared.Domain.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{ Task AddAsync(TEntity entity, CancellationToken cancellationToken = default); }
