using System.ComponentModel.DataAnnotations;

namespace CrmBack.Core.Models.Payload;

public record UpdateUserPayload
(
    [Required][StringLength(100)] string Name,
    [Required][StringLength(50)] string Login
);
