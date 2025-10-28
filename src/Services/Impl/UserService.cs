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

    public async Task<LoginResponseDto> Login(LoginUserDto dto, CancellationToken ct = default)
    {
        var user = await dao.FetchByLogin(dto, ct) 
            ?? throw new UnauthorizedAccessException("Invalid login or password");

        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        var (accessToken, refreshToken, accessTokenExpiresAt) = await CreateTokensAsync(user, ct);

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
        refreshToken ??= cookieService.GetRefreshTokenFromCookie() 
            ?? throw new UnauthorizedAccessException("Refresh token not found in cookies");

        var userId = jwt.GetUserIdFromRefreshToken(refreshToken) 
            ?? throw new UnauthorizedAccessException("Invalid refresh token format");
        
        var storedTokens = await refDao.GetUserTokensForValidationAsync(userId, ct);
        var storedToken = storedTokens.FirstOrDefault(token => BCrypt.Net.BCrypt.Verify(refreshToken, token.TokenHash))
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        var user = await dao.FetchByIdWithPolicies(storedToken.UsrId, ct) 
            ?? throw new UnauthorizedAccessException("User not found");
        
        await refDao.RevokeTokenByIdAsync(storedToken.RefreshTokenId, storedToken.UsrId, ct);
        
        var (accessToken, newRefreshToken, accessTokenExpiresAt) = await CreateTokensAsync(user, ct);
        
        return new RefreshTokenResponseDto { ExpiresAt = accessTokenExpiresAt };
    }

    private async Task<(string accessToken, string refreshToken, DateTime accessTokenExpiresAt)> CreateTokensAsync(
        UserWithPoliciesDto user, CancellationToken ct)
    {
        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        var accessToken = jwt.GenerateAccessToken(user.UsrId, user.Login, roles);
        var refreshToken = jwt.GenerateRefreshToken(user.UsrId);
        var refreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await refDao.CreateAsync(user.UsrId, refreshTokenHash, expiresAt, ct);
        
        cookieService.SetAccessTokenCookie(accessToken, accessTokenExpiresAt);
        cookieService.SetRefreshTokenCookie(refreshToken, expiresAt);

        return (accessToken, refreshToken, accessTokenExpiresAt);
    }

    public async Task<bool> Logout(int userId, CancellationToken ct = default)
    {
        var success = await refDao.RevokeAllUserTokensAsync(userId, ct);
        cookieService.ClearAuthCookies();
        return success;
    }
}
