namespace CrmBack.Core.Models.Payload.User;

using System.ComponentModel.DataAnnotations;

public record CreateUserPayload
(
    [Required]
    [MinLength(2, ErrorMessage = "First name cannot be less 2 character")]
    [MaxLength(20, ErrorMessage = "First name cannot be over 20 character")]
    string FirstName,
    [Required]
    [MinLength(2, ErrorMessage = "Last name cannot be less 2 character")]
    [MaxLength(20, ErrorMessage = "Last name cannot be over 20 character")]
    string LastName,
    [Required]
    [MinLength(2, ErrorMessage = "Middle name cannot be less 2 character")]
    [MaxLength(20, ErrorMessage = "Middle name cannot be over 20 character")]
    string MiddleName,
    [Required]
    [MinLength(5, ErrorMessage = "Login cannot be less 5 character")]
    [MaxLength(20, ErrorMessage = "Login cannot be over 20 character")]
    string Login,
    [Required]
    string Password
);