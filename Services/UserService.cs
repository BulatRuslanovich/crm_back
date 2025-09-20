namespace CrmBack.Services;

using System.Collections.Generic;
using CrmBack.Core.Models.Payload;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<ReadUserPayload> CreateUser(CreateUserPayload user)
    {
        var userId = await userRepository.CreateAsync(user.ToEntity()).ConfigureAwait(false);
        var userDto = await userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        return userDto?.ToReadPayload() ?? throw new InvalidOperationException("User was created but cannot be retrieved"); ;
    }

    public async Task<bool> DeleteUser(int id)
    {
        return await userRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ReadUserPayload>> GetAllUsers()
    {
        var users = await userRepository.GetAllAsync().ConfigureAwait(false) ?? throw new NullReferenceException("User not exist");

        return users.Select(u => u.ToReadPayload());
    }

    public async Task<ReadUserPayload> GetUserById(int id)
    {
        var user = await userRepository.GetByIdAsync(id).ConfigureAwait(false) ?? throw new NullReferenceException("User not exist");
        return user.ToReadPayload();
    }

    public async Task<bool> UpdateUser(int id, UpdateUserPayload user)
    {
        return await userRepository.UpdateAsync(user.ToEntity(id)).ConfigureAwait(false);
    }
}