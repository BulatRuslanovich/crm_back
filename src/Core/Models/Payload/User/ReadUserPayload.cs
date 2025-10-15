namespace CrmBack.Core.Models.Payload.User;

public record ReadUserPayload
(
    int Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Login
);

