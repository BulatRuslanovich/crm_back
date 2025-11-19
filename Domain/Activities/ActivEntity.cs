using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrmBack.Application.Activities.Dto;
using CrmBack.Domain.Common;
using CrmBack.Domain.Organizations;
using CrmBack.Domain.Users;

namespace CrmBack.Domain.Activities;

[Table("activ")]
public class ActivEntity : BaseEntity
{
	[Key]
	[Column("activ_id")]
	public int ActivId { get; set; }

	[Column("usr_id")]
	public int UsrId { get; set; }

	[Column("org_id")]
	public int OrgId { get; set; }

	[Column("status_id")]
	public int StatusId { get; set; }

	[Column("activ_date", TypeName = "date")]
	public DateTime VisitDate { get; set; }

	[Column("activ_starttime", TypeName = "time")]
	public TimeSpan StartTime { get; set; }

	[Column("activ_endtime", TypeName = "time")]
	public TimeSpan EndTime { get; set; }

	[Column("activ_description")]
	public string? Description { get; set; }

	[ForeignKey("UsrId")]
	public virtual UserEntity User { get; set; } = null!;

	[ForeignKey("OrgId")]
	public virtual OrgEntity Organization { get; set; } = null!;

	[ForeignKey("StatusId")]
	public virtual StatusEntity Status { get; set; } = null!;

	public void Update(UpdateActivDto dto)
	{
		StatusId = dto.StatusId ?? StatusId;
		VisitDate = dto.VisitDate ?? VisitDate;
		StartTime = dto.StartTime ?? StartTime;
		EndTime = dto.EndTime ?? EndTime;
		Description = dto.Description ?? Description;
	}
}
