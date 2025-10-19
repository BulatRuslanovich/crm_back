namespace CrmBack.Services.Impl;

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Utils.Mapper;
using CrmBack.Repository;
using Microsoft.IdentityModel.Tokens;

public class UserService(IUserRepository userRepository, IActivRepository activRepository, IPlanRepository planRepository, IOrgRepository orgRepository, IConfiguration configuration) : IUserService
{
    public async Task<ReadUserPayload?> GetById(int id, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return user?.ToReadPayload();
    }

    public async Task<List<ReadUserPayload>> GetAll(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(isDeleted, page, pageSize, ct).ConfigureAwait(false);

        return [.. users.Select(u => u.ToReadPayload())];
    }

    //! there's no point in checking the login's uniqueness, as the field is unique in the database
    public async Task<ReadUserPayload?> Create(CreateUserPayload payload, CancellationToken ct = default)
    {
        var userId = await userRepository.CreateAsync(payload.ToEntity(), ct).ConfigureAwait(false);
        var userDto = await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);
        return userDto?.ToReadPayload();
    }

    public async Task<bool> Update(int id, UpdateUserPayload payload, CancellationToken ct = default)
    {
        var existing = await userRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (existing == null) return false;

        var newEntity = new UserEntity(
            usr_id: id,
            first_name: payload.FirstName ?? existing.first_name,
            last_name: payload.LastName ?? existing.last_name,
            middle_name: payload.MiddleName ?? existing.middle_name,
            login: payload.Login ?? existing.login,
            password_hash: string.IsNullOrEmpty(payload.Password) ? existing.password_hash : BCrypt.Net.BCrypt.HashPassword(payload.Password),
            is_deleted: existing.is_deleted
        );

        return await userRepository.UpdateAsync(newEntity, ct).ConfigureAwait(false);
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await userRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);

    public async Task<LoginResponsePayload> Login(LoginUserPayload payload, CancellationToken ct = default)
    {
        var user = (await userRepository.FindByAsync("login", payload.Login, ct: ct)).FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Invalid login or password.");
        if (!BCrypt.Net.BCrypt.Verify(payload.Password, user.password_hash))
            throw new UnauthorizedAccessException("Invalid login or password.");

        var token = GenerateJwtToken(user);
        var userPayload = user.ToReadPayload();
        return new LoginResponsePayload(token, userPayload);
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "lol"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.usr_id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.login),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<List<HumReadActivPayload>> GetActivs(int userId, CancellationToken ct = default)
    {
        var humActivs = await activRepository.GetAllHumActivsByUserIdAsync(userId, ct);

        return humActivs.ToList();
    }

    public async Task<List<ReadPlanPayload>> GetPlans(int userId, CancellationToken ct = default)
    {
        var plans = await planRepository.FindByAsync("usr_id", userId, ct: ct);
        return [.. plans.Select(p => p.ToReadPayload())];
    }
}
