using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrmBack.Domain.Activities;
using CrmBack.Domain.Common;

namespace CrmBack.Domain.Organizations;

[Table("org")]
public class OrgEntity : BaseEntity
{
    [Key]
    [Column("org_id")]
    public int OrgId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("inn")]
    public string? Inn { get; set; } = string.Empty;

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("address")]
    public string Address { get; set; } = string.Empty;

    public virtual ICollection<ActivEntity> Activities { get; set; } = [];
}
