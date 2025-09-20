namespace CrmBack.Core.Models.Payload;

using System.ComponentModel.DataAnnotations;

public record CreateUserPayload
(
    [Required][StringLength(100)] string Name,
    [Required][StringLength(50)] string Login
);
