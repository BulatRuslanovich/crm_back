using BCrypt.Net;
using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Utils;
using CrmBack.DAO;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;


namespace CrmBack.Services.Impl;
public class UserService(IUserDAO dao, IJwtService jwt, IRefreshTokenDAO refDao, ICookieService cookie) : IUserService
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
        var user = await dao.FetchByLogin(dto, ct) ?? throw new UnauthorizedAccessException("Invalid login or password");

        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        await CreateTokensAsync(user, ct);

        return new LoginResponseDto
        {
            UserId = user.UsrId,
            Login = user.Login,
            Roles = roles
        };
    }

    public async Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default) =>
        await dao.FetchHumActivs(userId, ct);

    public async Task<bool> RefreshToken(string? refreshToken = null, CancellationToken ct = default)
    {
        refreshToken ??= cookie.GetRefreshTkn()
            ?? throw new UnauthorizedAccessException("Refresh token not found in cookies");

        int userId = jwt.GetUsrId(refreshToken) ?? throw new UnauthorizedAccessException("Invalid refresh token format");

        var tkns = await refDao.GetUserTokens(userId, ct);
        var tkn = tkns.FirstOrDefault(token => BCrypt.Net.BCrypt.Verify(refreshToken, token.TokenHash))
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        var user = await dao.FetchByIdWithPolicies(tkn.UsrId, ct) ?? throw new UnauthorizedAccessException("User not found");

        await refDao.RevokeById(tkn.RefreshTokenId, tkn.UsrId, ct);

        await CreateTokensAsync(user, ct);

        return true;
    }

    private async Task CreateTokensAsync(UserWithPoliciesDto user, CancellationToken ct)
    {
        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        string accessTkn = jwt.GenerateAccessTkn(user.UsrId, user.Login, roles);
        string refreshTkn = jwt.GenerateRefreshTkn(user.UsrId);
        string refreshTknHash = BCrypt.Net.BCrypt.HashPassword(refreshTkn);

        var expiresAt = DateTime.UtcNow.AddDays(7);
        var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await refDao.CreateAsync(user.UsrId, refreshTknHash, expiresAt, ct);

        cookie.SetAccessTkn(accessTkn, accessTokenExpiresAt);
        cookie.SetRefreshTkn(refreshTkn, expiresAt);
    }

    public async Task<bool> Logout(int userId, CancellationToken ct = default)
    {
        bool success = await refDao.RevokeAll(userId, ct);
        cookie.Clear();
        return success;
    }
}
