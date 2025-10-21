namespace CrmBack.Core.Models.Payload.User;

public record LoginResponsePayload(
    string AccessToken,
    string RefreshToken,
    ReadUserPayload User
);
