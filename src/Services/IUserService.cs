namespace CrmBack.Services;

using System.Threading;
using CrmBack.Core.Models.Dto;

public interface IUserService : IService<ReadUserDto, CreateUserDto, UpdateUserDto>
{
    public Task<LoginResponseDto> Login(LoginUserDto Dto, HttpContext httpContext, CancellationToken ct = default);
    public Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default);
    public Task<RefreshTokenResponseDto> RefreshToken(string refreshToken, CancellationToken ct = default);
    public Task<bool> RevokeToken(string refreshToken, CancellationToken ct = default);
    public Task<bool> Logout(int userId, CancellationToken ct = default);
    public Task<List<ActiveSessionDto>> GetActiveSessions(int userId, CancellationToken ct = default);
    public Task<bool> RevokeSession(int userId, int sessionId, CancellationToken ct = default);
}
