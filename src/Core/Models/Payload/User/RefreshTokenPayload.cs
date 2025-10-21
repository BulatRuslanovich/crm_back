namespace CrmBack.Core.Models.Payload.User;

public record RefreshTokenPayload(
    string RefreshToken,
    string AccessToken
);
