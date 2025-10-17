namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Plan;
using System.Threading;

public interface IPlanService
{
    public Task<ReadPlanPayload?> GetPlanById(int id, CancellationToken ct = default);
    public Task<List<ReadPlanPayload>> GetAllPlans(bool isDeleted, int page, int pageSize, CancellationToken ct = default);
    public Task<ReadPlanPayload?> CreatePlan(CreatePlanPayload payload, CancellationToken ct = default);
    public Task<bool> UpdatePlan(int id, UpdatePlanPayload payload, CancellationToken ct = default);
    public Task<bool> DeletePlan(int id, CancellationToken ct = default);
}