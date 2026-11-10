using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public abstract class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; } = context;
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
}
