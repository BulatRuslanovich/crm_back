using System.ComponentModel.DataAnnotations;
using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Dto;

public class CreatePlanDto
{
    [Required]
    public int UsrId { get; set; }
    [Required]
    public int OrgId { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
}

public class ReadPlanDto
{
    public int PlanId { get; set; }
    public int UsrId { get; set; }
    public int OrgId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdatePlanDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public static class PlanMapper
{
    public static ReadPlanDto ToReadDto(this PlanEntity entity) => new()
    {
        PlanId = entity.PlanId,
        UsrId = entity.UsrId,
        OrgId = entity.OrgId,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate
    };

    public static PlanEntity ToEntity(this CreatePlanDto dto) => new()
    {
        UsrId = dto.UsrId,
        OrgId = dto.OrgId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
    };
}
