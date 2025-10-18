namespace CrmBack.Core.Models.Payload.Activ;

public record UpdateActivPayload(
    int? StatusId,
    DateTime? VisitDate,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    string? Description
);
