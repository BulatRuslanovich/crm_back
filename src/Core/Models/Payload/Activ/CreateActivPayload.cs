namespace CrmBack.Core.Models.Payload.Activ;

public record CreateActivPayload(
    int UsrId,
    int OrgId,
    DateTime VisitDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Description
);