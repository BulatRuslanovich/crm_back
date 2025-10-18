namespace CrmBack.Services;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class UserService(IUserRepository userRepository, IConfiguration configuration) : IUserService
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

        var entityToUpdate = payload.ToEntity(id, existingLogin: existing.login, existingHash: existing.password_hash);
        return await userRepository.UpdateAsync(entityToUpdate, ct).ConfigureAwait(false);
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        return await userRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);
    }

    public async Task<LoginResponsePayload> Login(LoginUserPayload payload, CancellationToken ct = default)
    {
        var user = await userRepository.GetByLoginAsync(payload.Login, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(payload.Password, user.password_hash))
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
}