namespace CrmBack.Services;

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
    public async Task<ReadUserPayload?> GetUserById(int id)
    {
        var user = await userRepository.GetByIdAsync(id).ConfigureAwait(false);
        return user?.ToReadPayload();
    }

    public async Task<List<ReadUserPayload>> GetAllUsers()
    {
        var users = await userRepository.GetAllAsync().ConfigureAwait(false);

        return [.. users.Select(u => u.ToReadPayload())];
    }

    //! there's no point in checking the login's uniqueness, as the field is unique in the database
    public async Task<ReadUserPayload?> CreateUser(CreateUserPayload payload)
    {
        var userId = await userRepository.CreateAsync(payload.ToEntity()).ConfigureAwait(false);
        var userDto = await userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        return userDto?.ToReadPayload();
    }

    public async Task<bool> UpdateUser(int id, UpdateUserPayload payload)
    {
        return await userRepository.UpdateAsync(payload.ToEntity(id)).ConfigureAwait(false);
    }

    public async Task<bool> DeleteUser(int id)
    {
        return await userRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }

    public async Task<LoginResponsePayload> LoginUser(LoginUserPayload payload)
    {
        var user = await userRepository.GetByLoginAsync(payload.Login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(payload.Password, user.password_hash))
            throw new UnauthorizedAccessException("Invalid login or password.");


        var token = GenerateJwtToken(user);

        var userPayload = user.ToReadPayload();
        return new LoginResponsePayload(token, userPayload);
    }

    private string GenerateJwtToken(dynamic user)
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