using CrmBack.Domain.Activities;

namespace CrmBack.Application.Activities.Dto;

public record CreateActivDto(
	int UsrId,
	int OrgId,
	DateTime VisitDate,
	TimeSpan StartTime,
	TimeSpan EndTime,
	string? Description = null
);

public record HumReadActivDto(
	int ActivId,
	int UsrId,
	int OrgId,
	int StatusId,
	DateTime VisitDate,
	TimeSpan StartTime,
	TimeSpan EndTime,
	string? Description,
	string OrgName,
	string StatusName
) : ReadActivDto(ActivId, UsrId, OrgId, StatusId, VisitDate, StartTime, EndTime, Description);

public record ReadActivDto(
	int ActivId,
	int UsrId,
	int OrgId,
	int StatusId,
	DateTime VisitDate,
	TimeSpan StartTime,
	TimeSpan EndTime,
	string? Description
);

public record UpdateActivDto(
	int? StatusId = null,
	DateTime? VisitDate = null,
	TimeSpan? StartTime = null,
	TimeSpan? EndTime = null,
	string? Description = null
);

public static class ActivMapper
{
	public static ReadActivDto ToReadDto(this ActivEntity entity) =>
		new(entity.ActivId, entity.UsrId, entity.OrgId, entity.StatusId, entity.VisitDate, entity.StartTime, entity.EndTime, entity.Description ?? string.Empty);

	public static HumReadActivDto ToHumReadDto(this ActivEntity entity) =>
		new(entity.ActivId, entity.UsrId, entity.OrgId, entity.StatusId, entity.VisitDate, entity.StartTime, entity.EndTime, entity.Description ?? string.Empty,
			entity.Organization?.Name ?? string.Empty, entity.Status?.Name ?? string.Empty);

	public static ActivEntity ToEntity(this CreateActivDto payload) => new()
	{
		UsrId = payload.UsrId,
		OrgId = payload.OrgId,
		StatusId = 1,
		VisitDate = payload.VisitDate,
		StartTime = payload.StartTime,
		EndTime = payload.EndTime,
		Description = payload.Description,
	};
}


