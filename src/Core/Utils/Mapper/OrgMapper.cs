namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Org;

public static class OrgMapper
{
    public static ReadOrgPayload ToReadPayload(this OrgEntity entity)
    {
        return new ReadOrgPayload(
            OrgId: entity.org_id,
            Name: entity.name ?? "-",
            INN: entity.inn ?? "-",
            Latitude: entity.latitude ?? 0,
            Longitude: entity.longitude ?? 0,
            Address: entity.address ?? "-"
        );
    }

    public static List<ReadOrgPayload> ToReadPayloads(this IEnumerable<OrgEntity> entities)
    {
        return [.. entities.Select(ToReadPayload)];
    }

    public static OrgEntity ToEntity(this CreateOrgPayload payload)
    {
        return new OrgEntity(
            org_id: 0,
            name: payload.Name,
            inn: payload.INN,
            latitude: payload.Latitude,
            longitude: payload.Longitude,
            address: payload.Address
        );
    }

    public static OrgEntity ToEntity(this UpdateOrgPayload payload, int id)
    {
        return new OrgEntity(
            org_id: id,
            name: payload.Name,
            inn: payload.INN,
            latitude: payload.Latitude,
            longitude: payload.Longitude,
            address: payload.Address
        );
    }
}