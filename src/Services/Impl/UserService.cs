using BCrypt.Net;
using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Utils;
using CrmBack.DAO;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;


namespace CrmBack.Services.Impl;
public class UserService(IUserDAO dao, IJwtService jwt, IRefreshTokenDAO refDao, ICookieService cookieService) : IUserService
{
    public async Task<ReadUserDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    public async Task<List<ReadUserDto>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default) =>
        await dao.FetchAll(isDeleted, page, pageSize, searchTerm, ct);

    public async Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);


    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);


    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);

    public async Task<LoginResponseDto> Login(LoginUserDto dto, HttpContext httpContext, CancellationToken ct = default)
    {
        var user = await dao.FetchByLogin(dto, ct) ?? throw new UnauthorizedAccessException("Invalid login or password");

        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        var accessToken = jwt.GenerateAccessToken(user.UsrId, user.Login, roles);
        var refreshToken = jwt.GenerateRefreshToken(user.UsrId);
        var refreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);

        var ipAddress = NetworkHelper.GetClientIpAddress(httpContext);
        var deviceInfo = DeviceInfoHelper.ParseDeviceInfo(httpContext);

        var expiresAt = DateTime.UtcNow.AddDays(7);
        var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await refDao.CreateAsync(user.UsrId, refreshTokenHash, expiresAt, deviceInfo.ToJsonString(), ipAddress, ct);

        cookieService.SetAccessTokenCookie(accessToken, accessTokenExpiresAt);
        cookieService.SetRefreshTokenCookie(refreshToken, expiresAt);

        return new LoginResponseDto
        {
            UserId = user.UsrId,
            Login = user.Login,
            Roles = roles,
            ExpiresAt = accessTokenExpiresAt
        };
    }

    public async Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default) =>
        await dao.FetchHumActivs(userId, ct);

    public async Task<RefreshTokenResponseDto> RefreshToken(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            refreshToken = cookieService.GetRefreshTokenFromCookie() ?? "";
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token not found in cookies");
            }
        }

        var userId = jwt.GetUserIdFromRefreshToken(refreshToken) ?? throw new UnauthorizedAccessException("Invalid refresh token format");

        var storedTokens = await refDao.GetUserTokensForValidationAsync(userId, ct);
        RefreshTokenEntity? storedToken = null;
        
        foreach (var token in storedTokens)
        {
            if (BCrypt.Net.BCrypt.Verify(refreshToken, token.TokenHash))
            {
                storedToken = token;
                break;
            }
        }
        
        if (storedToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token");
        var user = await dao.FetchByIdWithPolicies(storedToken.UsrId, ct) ?? throw new UnauthorizedAccessException("User not found");
        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        var newAccessToken = jwt.GenerateAccessToken(user.UsrId, user.Login, roles);

        await refDao.RevokeTokenByIdAsync(storedToken.RefreshTokenId, storedToken.UsrId, ct);

        var newRefreshToken = jwt.GenerateRefreshToken(user.UsrId);
        var newRefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
        var newExpiresAt = DateTime.UtcNow.AddDays(7);
        var newAccessTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await refDao.CreateAsync(user.UsrId, newRefreshTokenHash, newExpiresAt, ct: ct);

        cookieService.SetAccessTokenCookie(newAccessToken, newAccessTokenExpiresAt);
        cookieService.SetRefreshTokenCookie(newRefreshToken, newExpiresAt);

        return new RefreshTokenResponseDto
        {
            ExpiresAt = newAccessTokenExpiresAt
        };
    }

    public async Task<bool> RevokeToken(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        return await refDao.RevokeTokenAsync(tokenHash, ct);
    }

    public async Task<bool> RevokeAllUserTokens(int userId, CancellationToken ct = default) =>
        await refDao.RevokeAllUserTokensAsync(userId, ct);


    public async Task<bool> Logout(int userId, CancellationToken ct = default)
    {
        var success = await refDao.RevokeAllUserTokensAsync(userId, ct);
        cookieService.ClearAuthCookies();
        return success;
    }

    public async Task<List<ActiveSessionDto>> GetActiveSessions(int userId, CancellationToken ct = default)
    {
        var tokens = await refDao.GetUserTokensAsync(userId, ct);

        return [.. tokens.Select(token => new ActiveSessionDto
        {
            RefreshTokenId = token.RefreshTokenId,
            DeviceInfo = token.DeviceInfo ?? "Unknown",
            IpAddress = token.IpAddress ?? "Unknown",
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiresAt,
            IsCurrentSession = false //TODO: Можно добавить логику для определения текущей сессии
        })];
    }

    public async Task<bool> RevokeSession(int userId, int sessionId, CancellationToken ct = default)
    {
        return await refDao.RevokeTokenByIdAsync(sessionId, userId, ct);
    }
}
