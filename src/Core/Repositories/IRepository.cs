namespace CrmBack.Core.Repositories;


public interface IRepository<K, T>
{
    public Task<T?> GetByIdAsync(K id);
    public Task<IEnumerable<T>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10);
    public Task<K> CreateAsync(T entity);
    public Task<bool> UpdateAsync(T entity);
    public Task<bool> SoftDeleteAsync(K id);
    public Task<bool> HardDeleteAsync(K id);
}
