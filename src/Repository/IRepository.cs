namespace CrmBack.Repository;

public interface IRepository<K, T>
{
    public Task<T?> GetByIdAsync(K id, CancellationToken ct = default);
    public Task<IEnumerable<T>> GetAllAsync(bool isDeleted, int page, int pageSize, CancellationToken ct = default);
    public Task<K> CreateAsync(T entity, CancellationToken ct = default);
    public Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
    public Task<bool> SoftDeleteAsync(K id, CancellationToken ct = default);
    public Task<bool> HardDeleteAsync(K id, CancellationToken ct = default);
    
    public Task<IEnumerable<T>> FindAllAsync(
        Dictionary<string, object>? filters = null,
        string? orderByColumn = null,
        bool orderByDescending = false,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);
        
    public Task<IEnumerable<T>> FindByAsync(
        string column,
        object value,
        bool exactMatch = true,
        string? orderByColumn = null,
        bool orderByDescending = false,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);
        
    public Task<IEnumerable<T>> FindByRangeAsync(
        string column,
        object? minValue = null,
        object? maxValue = null,
        string? orderByColumn = null,
        bool orderByDescending = false,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);
}
