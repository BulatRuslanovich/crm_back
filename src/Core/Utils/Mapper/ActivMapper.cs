namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;

public static class ActivMapper
{
    public static ReadActivPayload ToReadPayload(this ActivEntity entity) => new(
        ActivId: entity.activ_id,
        UsrId: entity.usr_id,
        OrgId: entity.org_id,
        StatusId: entity.status_id,
        VisitDate: entity.visit_date,
        StartTime: entity.start_time,
        EndTime: entity.end_time,
        Description: entity.description
    );

    public static ActivEntity ToEntity(this CreateActivPayload payload) => new(
        activ_id: 0,
        usr_id: payload.UsrId,
        org_id: payload.OrgId,
        status_id: 1,
        visit_date: payload.VisitDate,
        start_time: payload.StartTime,
        end_time: payload.EndTime,
        description: payload.Description,
        is_deleted: false
    );
}