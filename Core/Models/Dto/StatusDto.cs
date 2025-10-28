using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Dto;

public class ReadStatusDto
{
    public int StatusId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public static class StatusMapper
{
    public static ReadStatusDto ToReadDto(this StatusEntity entity) => new()
    {
        StatusId = entity.StatusId,
        Name = entity.Name
    };
}
