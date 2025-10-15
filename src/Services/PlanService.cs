namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlanService(IPlanRepository planRepository) : IPlanService
{
    public async Task<ReadPlanPayload?> CreatePlan(CreatePlanPayload payload)
    {
        var planId = await planRepository.CreateAsync(payload.ToEntity()).ConfigureAwait(false);
        var plan = await planRepository.GetByIdAsync(planId).ConfigureAwait(false);
        return plan?.ToReadPayload();
    }

    public async Task<bool> DeletePlan(int id)
    {
        return await planRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }

    public async Task<List<ReadPlanPayload>> GetAllPlans()
    {
        var plans = await planRepository.GetAllAsync().ConfigureAwait(false);
        return plans.ToReadPayloads();
    }

    public async Task<ReadPlanPayload?> GetPlanById(int id)
    {
        var plan = await planRepository.GetByIdAsync(id).ConfigureAwait(false);
        return plan?.ToReadPayload();
    }

    public async Task<bool> UpdatePlan(int id, UpdatePlanPayload payload)
    {
        return await planRepository.UpdateAsync(payload.ToEntity(id)).ConfigureAwait(false);
    }
}