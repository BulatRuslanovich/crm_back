using System.ComponentModel.DataAnnotations;
using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Dto;

public class CreateActivDto
{
    [Required]
    public int UsrId { get; set; }
    [Required]
    public int OrgId { get; set; }
    [Required]
    public DateTime VisitDate { get; set; }
    [Required]
    public TimeSpan StartTime { get; set; }
    [Required]
    public TimeSpan EndTime { get; set; }
    public string? Description { get; set; }
}

public class HumReadActivDto
{
    public int ActivId { get; set; }
    public int UsrId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Description { get; set; }
};

public class ReadActivDto
{
    public int ActivId { get; set; }
    public int UsrId { get; set; }
    public int OrgId { get; set; }
    public int StatusId { get; set; }
    public DateTime VisitDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Description { get; set; }
};

public class UpdateActivDto
{
    public int? StatusId { get; set; }
    public DateTime? VisitDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Description { get; set; }
};


public static class ActivMapper
{
    public static ReadActivDto ToReadDto(this ActivEntity entity) =>
         new()
         {
             ActivId = entity.ActivId,
             UsrId = entity.UsrId,
             OrgId = entity.OrgId,
             VisitDate = entity.VisitDate,
             StatusId = entity.StatusId,
             StartTime = entity.StartTime,
             EndTime = entity.EndTime,
             Description = entity.Description ?? string.Empty,
         };

    public static HumReadActivDto ToHumReadDto(this ActivEntity entity) =>
        new()
        {
            ActivId = entity.ActivId,
            UsrId = entity.UsrId,
            OrgName = entity.Organization?.Name ?? string.Empty,
            VisitDate = entity.VisitDate,
            StatusName = entity.Status?.Name ?? string.Empty,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Description = entity.Description ?? string.Empty,
        };


    public static ActivEntity ToEntity(this CreateActivDto payload) => new()
    {
        UsrId = payload.UsrId,
        OrgId = payload.OrgId,
        StatusId = 1, // По умолчанию первый статус
        VisitDate = payload.VisitDate,
        StartTime = payload.StartTime,
        EndTime = payload.EndTime,
        Description = payload.Description,
    };
}


