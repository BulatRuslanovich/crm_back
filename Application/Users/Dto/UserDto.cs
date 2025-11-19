using System.Collections.Generic;
using CrmBack.Domain.Users;

namespace CrmBack.Application.Users.Dto;

public record CreateUserDto(
	string FirstName,
	string LastName,
	string? MiddleName,
	string Login,
	string Password
);

public record ReadUserDto(
	int UsrId,
	string FirstName,
	string LastName,
	string? MiddleName,
	string Login
);

public record UpdateUserDto(
	string? FirstName = null,
	string? LastName = null,
	string? MiddleName = null,
	string? Login = null,
	string? Password = null,
	string? CurrentPassword = null
);

public record LoginUserDto(
	string Login,
	string Password
);

public record LoginResponseDto(
	int UserId,
	string Login,
	List<string> Roles
);

public record RefreshTokenResponseDto(
	int UserId,
	string Login,
	List<string> Roles,
	string Message = "Token refreshed successfully"
);

public record UserWithPoliciesDto(
	int UsrId,
	string FirstName,
	string LastName,
	string? MiddleName,
	string Login,
	List<PolicyDto> Policies
) : ReadUserDto(UsrId, FirstName, LastName, MiddleName, Login);

public record PolicyDto(
	int PolicyId,
	string PolicyName
);

public static class UserMapper
{
	public static ReadUserDto ToReadDto(this UserEntity entity) =>
		new(entity.UsrId, entity.FirstName, entity.LastName, entity.MiddleName, entity.Login);

	public static UserEntity ToEntity(this CreateUserDto dto) => new()
	{
		FirstName = dto.FirstName,
		LastName = dto.LastName,
		MiddleName = dto.MiddleName,
		Login = dto.Login,
		PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
	};

	public static PolicyDto ToReadDto(this PolicyEntity entity) =>
		new(entity.PolicyId, entity.PolicyName);
}
