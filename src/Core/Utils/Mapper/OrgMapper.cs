namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Org;

public static class OrgMapper
{
    public static ReadOrgPayload ToReadPayload(this OrgEntity entity) => new(
        OrgId: entity.org_id,
        Name: entity.name,
        INN: entity.inn,
        Latitude: entity.latitude,
        Longitude: entity.longitude,
        Address: entity.address
    );


    public static OrgEntity ToEntity(this CreateOrgPayload payload) => new(
        org_id: 0,
        name: payload.Name,
        inn: payload.INN,
        latitude: payload.Latitude,
        longitude: payload.Longitude,
        address: payload.Address,
        is_deleted: false
    );
}
