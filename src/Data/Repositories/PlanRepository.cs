namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class PlanRepository(IDbConnection dbConnection) : BaseRepository<PlanEntity>(dbConnection), IPlanRepository
{
    private const string SelectQuery = @"
        SELECT plan_id,
               usr_id,
               org_id,
               is_deleted
        FROM plan
        WHERE plan_id = {0} AND NOT is_deleted
        LIMIT 1";

    public async Task<PlanEntity?> GetByIdAsync(int id) =>
        await QuerySingleAsync(string.Format(SelectQuery, "@id"), id).ConfigureAwait(false);

    public Task<int> CreateAsync(PlanEntity entity)
    {
        // TODO: check it, may be not work
        const string sql = @"INSERT INTO plan ()
                            VALUES ()
                            RETURNING plan_id";

        return ExecuteScalarAsync(sql, entity);
    }

    public Task<IEnumerable<PlanEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = $@"SELECT plan_id,
                            usr_id,
                            org_id,
                            is_deleted
                    FROM plan
                    {(includeDeleted ? "" : "WHERE NOT is_deleted")}
                    LIMIT @PageSize OFFSET @Offset";

        return QueryAsync(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    public async Task<bool> UpdateAsync(PlanEntity entity)
    {
        var existing = await GetByIdAsync(entity.plan_id);

        if (existing == null) return false;

        var updated = new PlanEntity(
            plan_id: existing.plan_id,
            usr_id: existing.usr_id,
            org_id: existing.org_id
        );

        const string sql = @"UPDATE plan 
                            SET usr_id = @usr_id
                                org_id = @org_id
                            WHERE plan_id = @plan_id";
        return await ExecuteAsync(sql, updated);
    }

    public async Task<bool> HardDeleteAsync(int id) =>
        await ExecuteAsync("DELETE FROM plan WHERE plan_id = @id", id);

    public async Task<bool> SoftDeleteAsync(int id) =>
        await ExecuteAsync("UPDATE plan SET is_deleted = true WHERE plan_id = @id", id);
}