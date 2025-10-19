namespace CrmBack.Core.Models.Payload.Activ;

public record HumReadActivPayload(
    int ActivId,
    int UsrId,
    string OrgName,
    string StatusName,
    DateTime VisitDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Description
);
