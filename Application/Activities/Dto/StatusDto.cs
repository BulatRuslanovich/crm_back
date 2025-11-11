using CrmBack.Domain.Activities;

namespace CrmBack.Application.Activities.Dto;

public record ReadStatusDto(
	int StatusId,
	string Name
);

public static class StatusMapper
{
	public static ReadStatusDto ToReadDto(this StatusEntity entity) =>
		new(entity.StatusId, entity.Name);
}
