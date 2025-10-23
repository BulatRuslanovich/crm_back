using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("refresh_tokens")]
public class RefreshTokenEntity
{
    [Key]
    [Column("refresh_token_id")]
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

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_revoked")]
    public bool IsRevoked { get; set; } = false;

    [Column("device_info")]
    public string? DeviceInfo { get; set; }

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    // Navigation properties
    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;
}