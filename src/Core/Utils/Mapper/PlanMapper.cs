namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Plan;

public static class PlanMapper
{
    public static ReadPlanPayload ToReadPayload(this PlanEntity entity)
    {
        return new ReadPlanPayload(
            PlanId: entity.plan_id,
            UsrId: entity.usr_id,
            OrgId: entity.org_id,
            StartDate: entity.start_date,
            EndDate: entity.end_date
        );
    }

    public static List<ReadPlanPayload> ToReadPayloads(this IEnumerable<PlanEntity> entities)
    {
        return [.. entities.Select(ToReadPayload)];
    }

    public static PlanEntity ToEntity(this CreatePlanPayload payload)
    {
        return new PlanEntity(
            usr_id: payload.UsrId,
            org_id: payload.OrgId,
            start_date: payload.StartDate,
            end_date: payload.EndDate
        );
    }

    public static PlanEntity ToEntity(this UpdatePlanPayload payload, int id)
    {
        return new PlanEntity(
            plan_id: id,
            org_id: payload.OrgId,
            usr_id: payload.UsrId,
            start_date: payload.StartDate,
            end_date: payload.EndDate
        );
    }
}