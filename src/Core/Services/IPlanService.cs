namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Plan;

public interface IPlanService : IService<ReadPlanPayload, CreatePlanPayload, UpdatePlanPayload> { }