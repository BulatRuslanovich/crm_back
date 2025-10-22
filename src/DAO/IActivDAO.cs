using CrmBack.Core.Models.Dto;

namespace CrmBack.DAO;

public interface IActivDAO
{
    public Task<List<ReadActivDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);

    public Task<ReadActivDto?> FetchById(int id, CancellationToken ct);

    public Task<ReadActivDto?> Create(CreateActivDto dto, CancellationToken ct = default);

    public Task<bool> Update(int id, UpdateActivDto dto, CancellationToken ct = default);

    public Task<bool> Delete(int id, CancellationToken ct = default);

    public Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default);
}
