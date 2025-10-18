namespace CrmBack.Core.Models.Payload.Plan;

public record CreatePlanPayload(
    int UsrId,
    int OrgId,
    DateTime StartDate,
    DateTime EndDate
);
