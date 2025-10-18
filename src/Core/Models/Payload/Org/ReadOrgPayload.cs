namespace CrmBack.Core.Models.Payload.Org;

public record ReadOrgPayload(
    int OrgId,
    string Name,
    string INN,
    double Latitude,
    double Longitude,
    string Address
);
