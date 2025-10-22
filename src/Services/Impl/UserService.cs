using BCrypt.Net;
using CrmBack.Core.Models.Dto;
using CrmBack.DAO;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;


namespace CrmBack.Services.Impl;
public class UserService(IUserDAO dao, IJwtService jwt) : IUserService
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

        var accessToken = jwt.GenerateAccessToken(user.UsrId, user.Login, [.. user.Policies.Select(p => p.PolicyName)]);
        var refreshToken = jwt.GenerateRefreshToken();

        //TODO: save token in db

        return new LoginResponseDto{
            Token = accessToken,
            RefreshToken = refreshToken,
            UserId = user.UsrId,
            Login = user.Login,
            Roles = [.. user.Policies.Select(p => p.PolicyName)]
        };
    }

    public async Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default) =>
        await dao.FetchHumActivs(userId, ct);

}
