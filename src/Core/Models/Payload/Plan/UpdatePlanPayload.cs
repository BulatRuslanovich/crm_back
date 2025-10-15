namespace CrmBack.Core.Models.Payload.Plan;

public record UpdatePlanPayload(
    int UsrId = 0,
    int OrgId = 0,
    DateTime StartDate = default,
    DateTime EndDate = default
);