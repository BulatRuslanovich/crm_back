using System;

namespace CrmBack.Core.Models.Payload.Activ;

public record CreateActivPayload(
    int UsrId,
    int OrgId,
    DateTime VisitDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description
);
