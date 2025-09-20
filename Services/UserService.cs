namespace CrmBack.Services;

using System.Collections.Generic;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserEntity> CreateUser(CreateUserPayload user)
    {
        var userId = await userRepository.CreateAsync(user).ConfigureAwait(false);
        var userDto = await userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        return userDto ?? throw new InvalidOperationException("User was created but cannot be retrieved"); ;
    }

    public async Task<IEnumerable<ReadUserPayload>> GetAllUsers()
    {
        var users = await userRepository.GetAllAsync().ConfigureAwait(false) ?? throw new NullReferenceException("User not exist");

        return users.Select(u => u.ToReadPayload());
    }

    public async Task<ReadUserPayload> GetUserById(int id)
    {
        var user = await userRepository.GetByIdAsync(id).ConfigureAwait(false) ?? throw new NullReferenceException("User not exist");

        var userPayload = new ReadUserPayload(user.usr_id, user.name, user.login);

        return userPayload;
    }
    
}