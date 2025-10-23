using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("usr")]
public class NewBaseType
{
    [Column("middle_name")]
    [MaxLength(100)]
    public string? MiddleName { get; set; }
}

[Table("usr")]
public class UserEntity : NewBaseType
{
    [Key]
    [Column("usr_id")]
    public int UsrId { get; set; }

    [Column("first_name")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Column("login")]
    [MaxLength(100)]
    [Required]
    public string Login { get; set; } = string.Empty;

    [Column("password_hash")]
    [MaxLength(255)]
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // [Column("created_at")]
    // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // [Column("updated_at")]
    // public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<ActivEntity> Activities { get; set; } = [];
    public virtual ICollection<PlanEntity> Plans { get; set; } = [];
    public virtual ICollection<UserPolicyEntity> UserPolicies { get; set; } = [];
}
