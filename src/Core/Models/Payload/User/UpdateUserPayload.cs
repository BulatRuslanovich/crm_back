namespace CrmBack.Core.Models.Payload.User;

using System.ComponentModel.DataAnnotations;

public record UpdateUserPayload
(
    [MinLength(2, ErrorMessage = "First name cannot be less 2 character")]
    [MaxLength(20, ErrorMessage = "First name cannot be over 20 character")]
    string? FirstName,
    [MinLength(4, ErrorMessage = "Last name cannot be less 2 character")]
    [MaxLength(20, ErrorMessage = "Last name cannot be over 20 character")]
    string? LastName,
    [MinLength(2, ErrorMessage = "Middle name cannot be less 2 character")]
    [MaxLength(20, ErrorMessage = "Middle name cannot be over 20 character")]
    string? MiddleName,
    [MinLength(5, ErrorMessage = "Login cannot be less 5 character")]
    [MaxLength(20, ErrorMessage = "Login cannot be over 20 character")]
    string? Login,
    string? Password
);