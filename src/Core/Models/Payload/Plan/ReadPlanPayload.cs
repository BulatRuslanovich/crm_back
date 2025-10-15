namespace CrmBack.Core.Models.Payload.Plan;

public record ReadPlanPayload(
    int PlanId,
    int UsrId,
    int OrgId
);