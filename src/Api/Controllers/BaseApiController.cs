
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CrmBack.Api.Controllers;

public abstract class BaseApiController<RPayload, CPayload, UPayload>(IDistributedCache cache, string entityPrefix, IService<RPayload, CPayload, UPayload> service) : ControllerBase
{
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        return await GetDataFromCache(
            $"{entityPrefix}{id}",
            () => service.GetById(id, HttpContext.RequestAborted),
            TimeSpan.FromMinutes(5)
        );
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] bool isDeleted = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 1000) return BadRequest("Invalid pagination parameters");
        return await GetListFromCache(
            $"{entityPrefix}{isDeleted}_{page}_{pageSize}",
            async () => await service.GetAll(isDeleted, page, pageSize, HttpContext.RequestAborted),
            TimeSpan.FromMinutes(10)
        );
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var data = await service.Create(payload, HttpContext.RequestAborted);

        var idProp = data?.GetType().GetProperty("Id");

        if (idProp == null)
        {
            return Created(string.Empty, data);
        }
        
        var id = idProp.GetValue(data);
        return CreatedAtAction(nameof(GetById), new { id }, data);
    }
    
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid) return BadRequest();
        var updated = await service.Update(id, payload, HttpContext.RequestAborted);
        if (!updated) return NotFound();
        return Ok(true);
    }
    
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest();
        var deleted = await service.Delete(id, HttpContext.RequestAborted);
        if (!deleted) return NotFound();
        return NoContent();
    }
    

    protected bool ValidateId(int id) => id > 0;

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected async Task<ActionResult<RPayload>> GetDataFromCache(
        string key,
        Func<Task<RPayload?>> fetchData,
        TimeSpan expiration)
    {
        var cacheData = await cache.GetStringAsync(key, HttpContext.RequestAborted);

        if (!string.IsNullOrEmpty(cacheData))
        {
            var value = JsonSerializer.Deserialize<RPayload>(cacheData, CacheJsonOptions);
            return Ok(value);
        }

        var data = await fetchData();

        if (data == null)
        {
            return NotFound();
        }

        var cacheOpt = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var serData = JsonSerializer.Serialize(data, CacheJsonOptions);
        await cache.SetStringAsync(key, serData, cacheOpt, HttpContext.RequestAborted);
        return Ok(data);
    }

    protected async Task<ActionResult<List<RPayload>>> GetListFromCache(
        string key,
        Func<Task<List<RPayload>>> fetchData,
        TimeSpan expiration)
    {
        var cacheData = await cache.GetStringAsync(key, HttpContext.RequestAborted);

        if (!string.IsNullOrEmpty(cacheData))
        {
            var value = JsonSerializer.Deserialize<List<RPayload>>(cacheData, CacheJsonOptions);
            return Ok(value);
        }

        var data = await fetchData();

        var cacheOpt = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var serData = JsonSerializer.Serialize(data, CacheJsonOptions);
        await cache.SetStringAsync(key, serData, cacheOpt, HttpContext.RequestAborted);
        return Ok(data);
    }

    protected async Task CleanCache(string key) =>
        await cache.RemoveAsync(key, HttpContext.RequestAborted);
}