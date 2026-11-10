using System.Linq.Expressions;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Base repository for all repositories
/// </summary>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context;

    protected BaseRepository(AppDbContext context)
    {
        Context = context;
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public async Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync(new object?[] { ConvertKey(id) }, cancellationToken);
    }

    public async Task<TEntity?> FindByIdAsNoTrackingAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(BuildKeyEqualsPredicate(id), cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private IProperty KeyProperty =>
        Context.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!.Properties[0];

    /// <summary>
    ///     Converts a plain <c>int</c> id into whatever CLR type the entity's primary key actually declares —
    ///     a strongly-typed id struct, via its registered value converter, or the plain <c>int</c> itself
    ///     when the key has no converter at all.
    /// </summary>
    private object ConvertKey(int id)
    {
        var converter = KeyProperty.GetTypeMapping().Converter;
        return converter is null ? id : converter.ConvertFromProvider(id)!;
    }

    /// <summary>
    ///     Builds <c>entity => entity.Id == convertedKey</c> as an expression tree, since <typeparamref name="TEntity" />
    ///     is generic here and the key's real CLR type is only known at runtime, through <see cref="KeyProperty" />.
    /// </summary>
    private Expression<Func<TEntity, bool>> BuildKeyEqualsPredicate(int id)
    {
        var keyProperty = KeyProperty;
        var convertedKey = ConvertKey(id);

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var property = Expression.Property(parameter, keyProperty.Name);
        var constant = Expression.Constant(convertedKey, property.Type);
        var equals = Expression.Equal(property, constant);

        return Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
    }
}
