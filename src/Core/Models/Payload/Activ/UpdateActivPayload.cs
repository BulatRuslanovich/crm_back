using System;

namespace CrmBack.Core.Models.Payload.Activ;

public record UpdateActivPayload(
    int StatusId,
    DateTime VisitDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description
);
