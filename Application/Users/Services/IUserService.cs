namespace CrmBack.Application.Users.Services;

using System.Threading;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Services;
using CrmBack.Application.Users.Dto;

/// <summary>
/// User service interface
/// Extends IService with user-specific authentication operations
/// </summary>
public interface IUserService : IService<ReadUserDto, CreateUserDto, UpdateUserDto>
{
	/// <summary>Authenticate user and return login response with tokens</summary>
	public Task<LoginResponseDto> Login(LoginUserDto Dto, CancellationToken ct = default);

	/// <summary>Get activities for a specific user</summary>
	public Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default);

	/// <summary>Refresh access token using refresh token</summary>
	public Task<RefreshTokenResponseDto> RefreshToken(string? refreshToken = null, CancellationToken ct = default);

	/// <summary>Logout user and invalidate all refresh tokens</summary>
	public Task<bool> Logout(int userId, CancellationToken ct = default);
}
