using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;



[Table("usr")]
public class UserEntity : BaseEntity
{
    [Key]
    [Column("usr_id")]
    public int UsrId { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("middle_name")]
    public string? MiddleName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("login")]
    public string Login { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    public virtual ICollection<ActivEntity> Activities { get; set; } = [];
    public virtual ICollection<UserPolicyEntity> UserPolicies { get; set; } = [];
}
