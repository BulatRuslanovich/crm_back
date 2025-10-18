namespace CrmBack.Core.Models.Payload.Plan;

public record UpdatePlanPayload(
    int? UsrId,
    int? OrgId,
    DateTime? StartDate,
    DateTime? EndDate
);
