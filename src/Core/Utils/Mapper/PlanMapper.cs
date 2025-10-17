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

    public static List<ReadPlanPayload> ToReadPayloads(this IEnumerable<PlanEntity> entities) =>
        [.. entities.Select(ToReadPayload)];

    public static PlanEntity ToEntity(this CreatePlanPayload payload) => new(
        usr_id: payload.UsrId,
        org_id: payload.OrgId,
        start_date: payload.StartDate,
        end_date: payload.EndDate
    );

    public static PlanEntity ToEntity(this UpdatePlanPayload payload, int id) => new(
        plan_id: id,
        org_id: payload.OrgId,
        usr_id: payload.UsrId,
        start_date: payload.StartDate,
        end_date: payload.EndDate
    );
}