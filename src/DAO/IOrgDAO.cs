using CrmBack.Core.Models.Dto;

namespace CrmBack.DAO;

public interface IOrgDAO
{
    public Task<List<ReadOrgDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);

    public Task<ReadOrgDto?> FetchById(int id, CancellationToken ct);

    public Task<ReadOrgDto?> Create(CreateOrgDto dto, CancellationToken ct = default);

    public Task<bool> Update(int id, UpdateOrgDto dto, CancellationToken ct = default);

    public Task<bool> Delete(int id, CancellationToken ct = default);
}
