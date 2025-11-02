using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("refresh")]
public class RefreshTokenEntity : BaseEntity
{
    [Key]
    [Column("refresh_id")]
    public int RefreshTokenId { get; set; }

    [Column("usr_id")]
    public int UsrId { get; set; }

    [Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
    
    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;
}
