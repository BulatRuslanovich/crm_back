namespace CrmBack.Core.Models.Payload;

using System.ComponentModel.DataAnnotations;

public record ReadUserPayload
(
    [Required]int Id,
    [Required][StringLength(100)] string Name,
    [Required][StringLength(50)] string Login
);

