using CrmBack.Core.Models.Dto;
using CrmBack.DAO;
using CrmBack.Services;

namespace CrmBack.DAO.Impl;

public class CachedPlanDAO(IPlanDAO planDao, ITagCacheService cache, ICacheInvalidService invalid) : IPlanDAO
{
    private readonly TimeSpan expiration = TimeSpan.FromMinutes(15);
    private const string PlanTag = "plans";
    private const string PlanListTag = "plan-lists";

    public async Task<ReadPlanDto?> Create(CreatePlanDto dto, CancellationToken ct = default)
    {
        var result = await planDao.Create(dto, ct);
        
        if (result != null)
        {
            invalid.Enqueue([PlanTag, PlanListTag, $"plan:user:{result.UsrId}", $"plan:org:{result.OrgId}"], ct);
        }
        
        return result;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var result = await planDao.Delete(id, ct);
   
        if (result)
            invalid.Enqueue([$"plan:{id}", PlanTag, PlanListTag], ct);
        
        return result;
    }

    public async Task<List<ReadPlanDto>> FetchAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        string cacheKey = $"plans:all:{page}:{pageSize}:{searchTerm ?? "null"}";
        
        var cachedData = await cache.GetAsync<List<ReadPlanDto>>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await planDao.FetchAll(page, pageSize, searchTerm, ct);

        await cache.SetAsync(cacheKey, result, [PlanListTag], expiration, ct);

        return result;
    }

    public async Task<ReadPlanDto?> FetchById(int id, CancellationToken ct)
    {
        string cacheKey = $"plan:id:{id}";
        
        var cachedData = await cache.GetAsync<ReadPlanDto>(cacheKey, ct);
        if (cachedData != null) return cachedData;

        var result = await planDao.FetchById(id, ct);

        if (result != null)
        {
            await cache.SetAsync(cacheKey, result, [PlanTag, $"plan:{id}", $"plan:user:{result.UsrId}", $"plan:org:{result.OrgId}"], expiration, ct);
        }

        return result;
    }

    public async Task<bool> Update(int id, UpdatePlanDto dto, CancellationToken ct = default)
    {
        var result = await planDao.Update(id, dto, ct);
        
        if (result)
            invalid.Enqueue([$"plan:{id}", PlanTag, PlanListTag], ct);
        
        return result;
    }
}

