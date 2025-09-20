namespace CrmBack.Core.Models.Payload.User;

using System.ComponentModel.DataAnnotations;

public record UpdateUserPayload
(
    [Required][StringLength(100)] string FirstName,
    [Required][StringLength(100)] string LastName,
    [Required][StringLength(100)] string MiddleName,
    [Required][StringLength(50)] string Login,
    [Required][StringLength(50)] string Password
);
