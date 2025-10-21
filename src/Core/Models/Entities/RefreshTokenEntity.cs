using CrmBack.Core.Utils.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("refresh_tokens")]
public record RefreshTokenEntity(
    [Column(IsKey = true, IsUpdatable = false)] int token_id,
    [Column] int usr_id,
    [Column] string token_hash,
    [Column] DateTime expires_at,
    [Column] DateTime created_at,
    [Column] bool is_revoked
);
