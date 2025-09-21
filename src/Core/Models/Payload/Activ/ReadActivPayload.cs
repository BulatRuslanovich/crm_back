using System;

namespace CrmBack.Core.Models.Payload.Activ;

public record ReadActivPayload(
    int ActivId,
    int UsrId,
    int OrgId,
    int StatusId,
    DateTime VisitDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Description
);
