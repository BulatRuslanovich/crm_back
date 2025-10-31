namespace CrmBack.Services.Impl;

using CrmBack.Core.Models.Dto;
using CrmBack.DAO;

public class PlanService(IPlanDAO dao) : IPlanService
{
    public async Task<ReadPlanDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    public async Task<List<ReadPlanDto>> GetAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default) =>
        await dao.FetchAll(page, pageSize, searchTerm, ct);

    public async Task<ReadPlanDto?> Create(CreatePlanDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);

    public async Task<bool> Update(int id, UpdatePlanDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);

    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);
}
