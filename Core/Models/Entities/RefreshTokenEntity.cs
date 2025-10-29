using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("refresh")]
public class RefreshTokenEntity
{
    [Key]
    [Column("refresh_id")]
    public int RefreshTokenId { get; set; }

    [Column("usr_id")]
    [Required]
    public int UsrId { get; set; }

    [Column("token_hash")]
    [MaxLength(255)]
    [Required]
    public string TokenHash { get; set; } = string.Empty;

    [Column("expires_at")]
    [Required]
    public DateTime ExpiresAt { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;
}
