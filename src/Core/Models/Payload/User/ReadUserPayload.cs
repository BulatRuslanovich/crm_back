namespace CrmBack.Core.Models.Payload.User;

using System.ComponentModel.DataAnnotations;

public record ReadUserPayload
(
    int Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string Login
);

