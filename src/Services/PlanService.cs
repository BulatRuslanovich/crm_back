namespace CrmBack.Services;

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
        return plans.ToReadPayloads();
    }

    public async Task<ReadPlanPayload?> GetById(int id, CancellationToken ct = default)
    {
        var plan = await planRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return plan?.ToReadPayload();
    }

    public async Task<bool> Update(int id, UpdatePlanPayload payload, CancellationToken ct = default)
    {
        return await planRepository.UpdateAsync(payload.ToEntity(id), ct).ConfigureAwait(false);
    }
}