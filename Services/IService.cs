namespace CrmBack.Services;
public interface IService<RPayload, CPayload, UPayload>
{
    public Task<RPayload?> GetById(int id, CancellationToken ct = default);
    public Task<List<RPayload>> GetAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);
    public Task<RPayload?> Create(CPayload payload, CancellationToken ct = default);
    public Task<bool> Update(int id, UPayload payload, CancellationToken ct = default);
    public Task<bool> Delete(int id, CancellationToken ct = default);
}
