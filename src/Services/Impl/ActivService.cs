namespace CrmBack.Services.Impl;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Status;
using CrmBack.Core.Utils.Mapper;
using CrmBack.Repository;

public class ActivService(IActivRepository activRepository) : IActivService
{
    public async Task<ReadActivPayload?> GetById(int id, CancellationToken ct = default)
    {
        var activ = await activRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return activ?.ToReadPayload();
    }

    public async Task<List<ReadActivPayload>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var actives = await activRepository.GetAllAsync(isDeleted, page, pageSize, ct).ConfigureAwait(false);

        return [.. actives.Select(u => u.ToReadPayload())];
    }

    public async Task<ReadActivPayload?> Create(CreateActivPayload payload, CancellationToken ct = default)
    {
        var activId = await activRepository.CreateAsync(payload.ToEntity(), ct).ConfigureAwait(false);
        var activDto = await activRepository.GetByIdAsync(activId, ct).ConfigureAwait(false);
        return activDto?.ToReadPayload();
    }

    public async Task<bool> Update(int id, UpdateActivPayload payload, CancellationToken ct = default)
    {
        var existing = await activRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (existing == null) return false;

        var newEntity = new ActivEntity(
            activ_id: id,
            usr_id: existing.usr_id,
            org_id: existing.org_id,
            status_id: payload.StatusId ?? existing.status_id,
            visit_date: payload.VisitDate ?? existing.visit_date,
            start_time: payload.StartTime ?? existing.start_time,
            end_time: payload.EndTime ?? existing.end_time,
            description: payload.Description ?? existing.description,
            is_deleted: existing.is_deleted
        );

        return await activRepository.UpdateAsync(newEntity, ct).ConfigureAwait(false);
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        return await activRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);
    }

    public async Task<List<ReadActivPayload>> GetByUserId(int userId, CancellationToken ct = default)
    {
        var filters = new Dictionary<string, object> { { "usr_id", userId } };
        var activs = await activRepository.FindAllAsync(filters: filters, ct: ct);
        return [.. activs.Select(a => a.ToReadPayload())];
    }

    public async Task<List<ReadStatusPayload>> GetAllStatus(CancellationToken ct = default)
    {
        var statuses = await activRepository.GetAllStatusAsync(ct).ConfigureAwait(false);
        return [.. statuses.Select(s => s.ToReadPayload())];
    }
}
