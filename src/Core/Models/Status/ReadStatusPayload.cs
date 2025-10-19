using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Status;

public record ReadStatusPayload(
    int StatusId,
    string Name
);

public static class ReadStatusPayloadExtensions
{
    public static ReadStatusPayload ToReadPayload(this StatusEntity entity) => new(
        StatusId: entity.status_id,
        Name: entity.name
    );
}
