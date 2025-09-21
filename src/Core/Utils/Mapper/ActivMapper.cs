using System;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;

namespace CrmBack.Core.Utils.Mapper;

public static class ActivMapper
{
    public static ReadActivPayload ToReadPayload(this ActivEntity entity)
    {
        return new ReadActivPayload(
            ActivId: entity.activ_id,
            UsrId: entity.usr_id,
            OrgId: entity.org_id,
            StatusId: entity.status_id,
            VisitDate: entity.visit_date,
            StartTime: entity.start_time,
            EndTime: entity.end_time,
            Description: entity.description
        );
    }

    public static IEnumerable<ReadActivPayload> ToReadPayloads(this IEnumerable<ActivEntity> entities)
    {
        return entities.Select(ToReadPayload);
    }

    public static ActivEntity ToEntity(this CreateActivPayload payload)
    {
        return new ActivEntity(
            usr_id: payload.UsrId,
            org_id: payload.OrgId,
            visit_date: payload.VisitDate,
            start_time: payload.StartTime,
            end_time: payload.EndTime,
            description: payload.Description            
        );
    }

    public static ActivEntity ToEntity(this UpdateActivPayload payload, int id)
    {
        return new ActivEntity(
            activ_id: id,
            status_id: payload.StatusId,
            visit_date: payload.VisitDate,
            start_time: payload.StartTime,
            end_time: payload.EndTime,
            description: payload.Description 
        );
    }
}
