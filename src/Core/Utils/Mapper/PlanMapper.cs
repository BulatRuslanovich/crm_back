namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Plan;

public static class PlanMapper
{
    public static ReadPlanPayload ToReadPayload(this PlanEntity entity) => new(
        PlanId: entity.plan_id,
        UsrId: entity.usr_id,
        OrgId: entity.org_id,
        StartDate: entity.start_date,
        EndDate: entity.end_date
    );

    public static PlanEntity ToEntity(this CreatePlanPayload payload) => new(
        plan_id: 0,
        usr_id: payload.UsrId,
        org_id: payload.OrgId,
        start_date: payload.StartDate,
        end_date: payload.EndDate,
        is_deleted: false
    );
}
