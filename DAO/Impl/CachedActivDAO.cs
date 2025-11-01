using CrmBack.Core.Models.Dto;
using CrmBack.DAO;
using CrmBack.Services;

namespace CrmBack.DAO.Impl;

public class CachedActivDAO(IActivDAO activDao, ITagCacheService cache, ICacheInvalidService invalid) : IActivDAO
{
    private readonly TimeSpan expiration = TimeSpan.FromMinutes(10);
    private const string ActivTag = "activs";
    private const string ActivListTag = "activ-lists";

    public async Task<ReadActivDto?> Create(CreateActivDto dto, CancellationToken ct = default)
    {
        var result = await activDao.Create(dto, ct);
        
        if (result != null)
        {
            invalid.Enqueue([ActivTag, ActivListTag, $"activ:user:{result.UsrId}"], ct);
        }
        
        return result;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var result = await activDao.Delete(id, ct);
   
        if (result)
            invalid.Enqueue([$"activ:{id}", ActivTag, ActivListTag], ct);
        
        return result;
    }

    public async Task<List<ReadActivDto>> FetchAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        string cacheKey = $"activs:all:{page}:{pageSize}:{searchTerm ?? "null"}";
        
        var cachedData = await cache.GetAsync<List<ReadActivDto>>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await activDao.FetchAll(page, pageSize, searchTerm, ct);

        await cache.SetAsync(cacheKey, result, [ActivListTag], expiration, ct);

        return result;
    }

    public async Task<ReadActivDto?> FetchById(int id, CancellationToken ct)
    {
        string cacheKey = $"activ:id:{id}";
        
        var cachedData = await cache.GetAsync<ReadActivDto>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await activDao.FetchById(id, ct);

        if (result != null)
        {
            await cache.SetAsync(cacheKey, result, [ActivTag, $"activ:{id}", $"activ:user:{result.UsrId}"], expiration, ct);
        }

        return result;
    }

    public async Task<bool> Update(int id, UpdateActivDto dto, CancellationToken ct = default)
    {
        var result = await activDao.Update(id, dto, ct);
        
        if (result)
            invalid.Enqueue([$"activ:{id}", ActivTag, ActivListTag], ct);
        
        return result;
    }

    public async Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default)
    {
        string cacheKey = $"activs:user:{userId}";
        
        var cachedData = await cache.GetAsync<List<ReadActivDto>>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await activDao.FetchByUserId(userId, ct);

        await cache.SetAsync(cacheKey, result, [$"activ:user:{userId}", ActivListTag], expiration, ct);

        return result;
    }
}

