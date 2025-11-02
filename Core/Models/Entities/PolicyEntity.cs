using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("policy")]
public class PolicyEntity : BaseEntity
{
    [Key]
    [Column("policy_id")]
    public int PolicyId { get; set; }

    [Column("policy_name")]
    public string PolicyName { get; set; } = string.Empty;

    public virtual ICollection<UserPolicyEntity> UserPolicies { get; set; } = [];
}
