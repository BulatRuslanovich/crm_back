namespace CrmBack.Core.Models.Payload.Activ;

public record UpdateActivPayload(
    int? StatusId = null,
    DateTime? VisitDate = null,
    TimeSpan? StartTime = null,
    TimeSpan? EndTime = null,
    string? Description = null
);