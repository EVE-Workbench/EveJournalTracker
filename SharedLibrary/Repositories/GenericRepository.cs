using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Repositories.Interfaces;

namespace Common.Repositories;

public class GenericRepository<T, TId> : IGenericRepository<T, TId> where T : class
{
    private readonly DbContext _db;
    private readonly ILogger _logger;

    public GenericRepository(DbContext db, ILoggerFactory loggerFactory)
    {
        _db = db;
        _logger = loggerFactory.CreateLogger(nameof(GenericRepository<T, TId>));
    }

    ~GenericRepository()
    {
        Dispose(false);
    }

    #region Dispose implementation

    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        if (disposing)
        {
            // free managed resources
        }

        _isDisposed = true;
    }

    #endregion

    public virtual IQueryable<T> GetBaseQuery(bool tracking = true)
    {
        var query = _db.Set<T>().AsQueryable();
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    public async Task<T?> GetByIdAsync(TId id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var result =
            await _db.FindAsync<T>([id],
                cancellationToken: cancellationToken); // .Set<T>().FindAsync([id], cancellationToken: cancellationToken);
        if (!tracking && result != null)
        {
            _db.Entry(result).State = EntityState.Detached;
        }

        return result;
    }

    public virtual async Task<List<T>> GetAllAsync(bool tracking = false, CancellationToken cancellationToken = default)
    {
        return !tracking
            ? await _db.Set<T>().AsNoTracking().ToListAsync(cancellationToken: cancellationToken)
            : await _db.Set<T>().ToListAsync(cancellationToken: cancellationToken);
    }

    public virtual async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Set<T>().CountAsync(cancellationToken: cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetByExpressionAsync(Expression<Func<T, bool>> expression,
        bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<T>().Where(expression);
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(cancellationToken: cancellationToken);
    }

    public virtual async Task<bool> AddAsync(T entity, bool directSave = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.Set<T>().AddAsync(entity, cancellationToken);
            if (directSave)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during adding entity");
        }

        return false;
    }

    public virtual async Task<bool> AddRangeAsync(IEnumerable<T> entities, bool directSave = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.Set<T>().AddRangeAsync(entities, cancellationToken);
            if (directSave)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during adding entities");
        }

        return false;
    }

    public async Task<bool> UpdateAsync(T entity, bool directSave = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_db.Entry(entity).State == EntityState.Detached)
            {
                var original = await _db.Set<T>().FindAsync(GetKey<TId>(entity));
                if (original != null)
                {
                    _db.Entry(original).CurrentValues.SetValues(entity);
                }
            }
            else
            {
                _db.Entry(entity).OriginalValues.SetValues(entity);
            }

            if (directSave)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during updating entity");
        }

        return false;
    }

    public virtual bool Remove(T entity, bool directSave = true)
    {
        try
        {
            _db.Set<T>().Remove(entity);

            if (directSave)
            {
                _db.SaveChanges();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during entity removal");
        }

        return false;
    }

    public virtual bool RemoveRange(IEnumerable<T> entities, bool directSave = true)
    {
        try
        {
            _db.Set<T>().RemoveRange(entities);

            if (directSave)
            {
                _db.SaveChanges();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during removal of multiple entities");
        }

        return false;
    }

    public virtual async Task<bool> UpsertAsync(T entity, bool directSave = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // check if the entity is not already tracked
            if (!_EntityIsTracked(entity))
            {
                // check if the entity is already tracked
                if (_db.Entry(entity).State == EntityState.Detached)
                {
                    // check if there is a key value
                    var key = GetKey<TId>(entity);
                    if (key!.Equals(default(TId)))
                    {
                        await _db.Set<T>().AddAsync(entity, cancellationToken);
                    }
                    else
                    {
                        _db.Set<T>().Update(entity);
                    }
                }

                if (directSave)
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return true;
            }

            await UpdateAsync(entity, false, cancellationToken);

            if (directSave)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during upsert");
        }

        return false;
    }

    private bool _EntityIsTracked(T entity)
    {
        return _db.Entry(entity).State != EntityState.Detached;
    }

    public virtual TKey GetKey<TKey>(T entity)
    {
        var keyName = _db.Model.FindEntityType(typeof(T))
            ?.FindPrimaryKey()
            ?.Properties
            .Select(x => x.Name).Single();

        if (string.IsNullOrEmpty(keyName))
        {
            return default!;
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        return ((TKey)entity.GetType().GetProperty(keyName)?.GetValue(entity, null) ?? default(TKey)) ??
               throw new InvalidOperationException();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }

    public virtual async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during saving");
        }

        return false;
    }
}