namespace CrmBack.Core.Models.Payload.User;

public record LoginResponsePayload(
    string token,
    ReadUserPayload user
);
