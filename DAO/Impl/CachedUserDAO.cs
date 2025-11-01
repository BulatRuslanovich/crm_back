using CrmBack.Core.Models.Dto;
using CrmBack.Services;

namespace CrmBack.DAO.Impl;


public class CachedUserDAO(IUserDAO userDao, ITagCacheService cache, ICacheInvalidService invalid) : IUserDAO
{
    private readonly TimeSpan expiration = TimeSpan.FromMinutes(15);
    private const string UserTag = "users";
    private const string UserListTag = "user-lists";

    public async Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default)
    {
        var result = await userDao.Create(dto, ct);
        
        if (result != null)
            invalid.Enqueue([UserTag, UserListTag], ct);
        
        return result;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var result = await userDao.Delete(id, ct);
  
        if (result)
            invalid.Enqueue([$"user:{id}", UserTag, UserListTag], ct);
        
        return result;
    }

    public async Task<List<ReadUserDto>> FetchAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        string cacheKey = $"users:all:{page}:{pageSize}:{searchTerm ?? "null"}";
        
        var cachedData = await cache.GetAsync<List<ReadUserDto>>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await userDao.FetchAll(page, pageSize, searchTerm, ct);

        await cache.SetAsync(cacheKey, result, [UserListTag], expiration, ct);

        return result;
    }

    public async Task<ReadUserDto?> FetchById(int id, CancellationToken ct)
    {
        string cacheKey = $"user:id:{id}";
        
        var cachedData = await cache.GetAsync<ReadUserDto>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await userDao.FetchById(id, ct);

        if (result != null)
            await cache.SetAsync(cacheKey, result, [UserTag, $"user:{id}"], expiration, ct);

        return result;
    }

    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var result = await userDao.Update(id, dto, ct);
        
        if (result)
            invalid.Enqueue([$"user:{id}", UserTag, UserListTag], ct);
        
        return result;
    }

    public async Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default)
    {
        return await userDao.FetchHumActivs(userId, ct);
    }

    public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
    {
        return await userDao.FetchByLogin(dto, ct);
    }

    public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
    {
        return await userDao.FetchByIdWithPolicies(id, ct);
    }
}
