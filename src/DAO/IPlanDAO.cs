using CrmBack.Core.Models.Dto;


namespace CrmBack.DAO;

public interface IPlanDAO
{
    public Task<List<ReadPlanDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);

    public Task<ReadPlanDto?> FetchById(int id, CancellationToken ct);

    public Task<ReadPlanDto?> Create(CreatePlanDto dto, CancellationToken ct = default);

    public Task<bool> Update(int id, UpdatePlanDto dto, CancellationToken ct = default);

    public Task<bool> Delete(int id, CancellationToken ct = default);
}
