namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Options;

public class PlanRepository(IDbConnection dbConnection,
ILogger<ActivRepository> logger,
  IOptions<DatabaseLoggingOptions> loggingOptions) : BaseRepository<PlanEntity>(dbConnection, logger, loggingOptions), IPlanRepository
{
    private const string SelectQuery = @"
        SELECT plan_id,
               plan_name,
               is_deleted
        FROM plan
        WHERE {0} AND NOT is_deleted
        LIMIT 1";


    public async Task<int> CreateAsync(PlanEntity entity)
    {
        throw new NotImplementedException();
    }

    
    public Task<IEnumerable<PlanEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public async Task<PlanEntity?> GetByIdAsync(int id) =>
        await QuerySingleAsync(string.Format(SelectQuery, "plan_id = @id"), id).ConfigureAwait(false);

    public async Task<bool> HardDeleteAsync(int id) =>
    await ExecuteAsync("DELETE FROM plan WHERE plan_id = @Id", new { Id = id });

    public async Task<bool> SoftDeleteAsync(int id)
    => await ExecuteAsync("UPDATE plan SET is_deleted = true WHERE plan_id = @Id", new { Id = id });

    public Task<bool> UpdateAsync(PlanEntity entity)
    {
        throw new NotImplementedException();
    }
}