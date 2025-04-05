using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary.Repositories.Interfaces;

public interface IGenericRepository<T, in TId> : IDisposable where T : class
{
    IQueryable<T> GetBaseQuery(bool tracking = true);
    
    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetByIdAsync(TId id, bool tracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<T>> GetAllAsync(bool tracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts all entities
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get entities by expression
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<T>> GetByExpressionAsync(Expression<Func<T, bool>> expression, bool tracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add single entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="directSave"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> AddAsync(T entity, bool directSave = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add multiple entities
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="directSave"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> AddRangeAsync(IEnumerable<T> entities, bool directSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="directSave">Save changes immediatly</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(T entity, bool directSave = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove single entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="directSave"></param>
    /// <returns></returns>
    bool Remove(T entity, bool directSave = true);
    
    /// <summary>
    /// Remove multiple entities
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="directSave"></param>
    /// <returns></returns>
    bool RemoveRange(IEnumerable<T> entities, bool directSave = true);
    
    /// <summary>
    /// Save changes in the context
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert / Update entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="directSave"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpsertAsync(T entity, bool directSave = true, CancellationToken cancellationToken = default);
}