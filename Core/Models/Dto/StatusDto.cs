using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Dto;

public record ReadStatusDto(
    int StatusId,
    string Name
);

public static class StatusMapper
{
    public static ReadStatusDto ToReadDto(this StatusEntity entity) =>
        new(entity.StatusId, entity.Name);
}
