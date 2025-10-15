namespace CrmBack.Core.Models.Payload.Org;

public record UpdateOrgPayload(
    string? Name,
    string? INN,
    double? Latitude,
    double? Longitude,
    string? Address
);
