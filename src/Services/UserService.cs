namespace CrmBack.Services;

using System.Collections.Generic;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<ReadUserPayload?> GetUserById(int id)
    {
        var user = await userRepository.GetByIdAsync(id).ConfigureAwait(false);
        return user?.ToReadPayload();
    }

    public async Task<IEnumerable<ReadUserPayload>> GetAllUsers()
    {
        var users = await userRepository.GetAllAsync().ConfigureAwait(false);

        return users.Select(u => u.ToReadPayload());
    }

    public async Task<ReadUserPayload?> CreateUser(CreateUserPayload payload)
    {
        var userId = await userRepository.CreateAsync(payload.ToEntity()).ConfigureAwait(false);
        var userDto = await userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        return userDto?.ToReadPayload() ;
    }

    public async Task<bool> UpdateUser(int id, UpdateUserPayload payload)
    {
        return await userRepository.UpdateAsync(payload.ToEntity(id)).ConfigureAwait(false);
    }

    public async Task<bool> DeleteUser(int id)
    {
        return await userRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }
}