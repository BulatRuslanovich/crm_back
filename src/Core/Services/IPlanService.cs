namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Plan;

public interface IPlanService
{
    public Task<ReadPlanPayload?> GetPlanById(int id);
    public Task<List<ReadPlanPayload>> GetAllPlans(bool isDeleted, int page, int pageSize);
    public Task<ReadPlanPayload?> CreatePlan(CreatePlanPayload payload);
    public Task<bool> UpdatePlan(int id, UpdatePlanPayload payload);
    public Task<bool> DeletePlan(int id);
}