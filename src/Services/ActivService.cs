namespace CrmBack.Services;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class ActivService(IActivRepository activRepository) : IActivService
{
    public async Task<ReadActivPayload?> GetById(int id, CancellationToken ct = default)
    {
        var activ = await activRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return activ?.ToReadPayload();
    }

    public async Task<List<ReadActivPayload>> GetAll(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
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
}