namespace CrmBack.Core.Models.Payload.User;

public record LoginResponsePayload(
    string Token,
    ReadUserPayload User
);
