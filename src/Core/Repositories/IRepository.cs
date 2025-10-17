namespace CrmBack.Core.Repositories;


public interface IRepository<K, T>
{
    public Task<T?> GetByIdAsync(K id, CancellationToken ct = default);
    public Task<IEnumerable<T>> GetAllAsync(bool isDeleted, int page, int pageSize, CancellationToken ct = default);
    public Task<K> CreateAsync(T entity, CancellationToken ct = default);
    public Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
    public Task<bool> SoftDeleteAsync(K id, CancellationToken ct = default);
    public Task<bool> HardDeleteAsync(K id, CancellationToken ct = default);
}