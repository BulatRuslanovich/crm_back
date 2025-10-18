namespace CrmBack.Services;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlanService(IPlanRepository planRepository) : IPlanService
{
    public async Task<ReadPlanPayload?> Create(CreatePlanPayload payload, CancellationToken ct = default)
    {
        var planId = await planRepository.CreateAsync(payload.ToEntity(), ct).ConfigureAwait(false);
        var plan = await planRepository.GetByIdAsync(planId, ct).ConfigureAwait(false);
        return plan?.ToReadPayload();
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        return await planRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);
    }

    public async Task<List<ReadPlanPayload>> GetAll(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var plans = await planRepository.GetAllAsync(isDeleted, page, pageSize, ct).ConfigureAwait(false);
        return [.. plans.Select(p => p.ToReadPayload())];
    }

    public async Task<ReadPlanPayload?> GetById(int id, CancellationToken ct = default)
    {
        var plan = await planRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return plan?.ToReadPayload();
    }

    public async Task<bool> Update(int id, UpdatePlanPayload payload, CancellationToken ct = default)
    {

        var existing = await planRepository.GetByIdAsync(id, ct).ConfigureAwait(false);

        if (existing == null) return false;

        var newEntity = new PlanEntity(
            plan_id: id,
            usr_id: payload.UsrId ?? existing.usr_id,
            org_id: payload.OrgId ?? existing.org_id,
            start_date: payload.StartDate ?? existing.start_date,
            end_date: payload.EndDate ?? existing.end_date,
            is_deleted: existing.is_deleted
        );

        return await planRepository.UpdateAsync(newEntity, ct).ConfigureAwait(false);
    }
}