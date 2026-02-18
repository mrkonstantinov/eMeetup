using System.Linq.Expressions;

namespace eMeetup.Common.Application.Data;

public interface IRepository<T> where T : class
{
    // Query
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Command
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void UpdateRange(IEnumerable<T> entities);
    void DeleteRange(IEnumerable<T> entities);

    // Save
    Task<int> SaveChangesAsync();

    // Specialized query with includes
    IQueryable<T> Query();
}
