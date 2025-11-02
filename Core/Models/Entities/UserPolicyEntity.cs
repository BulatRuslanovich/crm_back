using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("usr_policy")]
public class UserPolicyEntity
{
    [Key]
    [Column("usr_policy_id")]
    public int UsrPolicyId { get; set; }

    [Column("usr_id")]
    public int UsrId { get; set; }

    [Column("policy_id")]
    public int PolicyId { get; set; }

    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;

    [ForeignKey("PolicyId")]
    public virtual PolicyEntity Policy { get; set; } = null!;
}
