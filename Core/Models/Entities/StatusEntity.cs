using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("status")]
public class StatusEntity
{
    [Key]
    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<ActivEntity> Activities { get; set; } = [];
}
