using System;

namespace CrmBack.Core.Models.Payload.Org;

public record CreateOrgPayload(
    string Name,
    string INN,
    double Latitude,
    double Longitude,
    string Address
);
