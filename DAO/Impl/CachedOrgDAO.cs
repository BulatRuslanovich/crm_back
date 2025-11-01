using CrmBack.Core.Models.Dto;
using CrmBack.DAO;
using CrmBack.Services;

namespace CrmBack.DAO.Impl;

public class CachedOrgDAO(IOrgDAO orgDao, ITagCacheService cache, ICacheInvalidService invalid) : IOrgDAO
{
    private readonly TimeSpan expiration = TimeSpan.FromMinutes(15);
    private const string OrgTag = "orgs";
    private const string OrgListTag = "org-lists";

    public async Task<ReadOrgDto?> Create(CreateOrgDto dto, CancellationToken ct = default)
    {
        var result = await orgDao.Create(dto, ct);
        
        if (result != null)
            invalid.Enqueue([OrgTag, OrgListTag], ct);
        
        return result;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var result = await orgDao.Delete(id, ct);
   
        if (result)
            invalid.Enqueue([$"org:{id}", OrgTag, OrgListTag], ct);
        
        return result;
    }

    public async Task<List<ReadOrgDto>> FetchAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        string cacheKey = $"orgs:all:{page}:{pageSize}:{searchTerm ?? "null"}";
        
        var cachedData = await cache.GetAsync<List<ReadOrgDto>>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await orgDao.FetchAll(page, pageSize, searchTerm, ct);

        await cache.SetAsync(cacheKey, result, [OrgListTag], expiration, ct);

        return result;
    }

    public async Task<ReadOrgDto?> FetchById(int id, CancellationToken ct)
    {
        string cacheKey = $"org:id:{id}";
        
        var cachedData = await cache.GetAsync<ReadOrgDto>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await orgDao.FetchById(id, ct);

        if (result != null)
            await cache.SetAsync(cacheKey, result, [OrgTag, $"org:{id}"], expiration, ct);

        return result;
    }

    public async Task<bool> Update(int id, UpdateOrgDto dto, CancellationToken ct = default)
    {
        var result = await orgDao.Update(id, dto, ct);
        
        if (result)
            invalid.Enqueue([$"org:{id}", OrgTag, OrgListTag], ct);
        
        return result;
    }
}

