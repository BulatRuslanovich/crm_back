using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Dto;

public class CreateUserDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string? MiddleName { get; set; }
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ReadUserDto
{
    public int UsrId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Login { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? CurrentPassword { get; set; }
}

public class LoginUserDto
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
}

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RevokeTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ActiveSessionDto
{
    public int RefreshTokenId { get; set; }
    public string DeviceInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCurrentSession { get; set; }
}

public class UserWithPoliciesDto : ReadUserDto
{
    public List<PolicyDto> Policies { get; set; } = [];
}

public class PolicyDto
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
}



public static class UserMapper
{
    public static ReadUserDto ToReadDto(this UserEntity entity) => new()
    {
        UsrId = entity.UsrId,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        MiddleName = entity.MiddleName,
        Login = entity.Login
    };

    public static UserEntity ToEntity(this CreateUserDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        MiddleName = dto.MiddleName,
        Login = dto.Login,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
    };

    public static PolicyDto ToReadDto(this PolicyEntity entity) => new() {
        PolicyId = entity.PolicyId,
        PolicyName = entity.PolicyName
    };
}
