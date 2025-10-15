namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class ActivRepository(IDbConnection dbConnection) : BaseRepository<ActivEntity>(dbConnection), IActivRepository
{
    private const string SelectQuery = @"
        SELECT activ_id,
               usr_id,
               org_id,
               status_id,
               visit_date,
               start_time,
               end_time,
               description,
               is_deleted
        FROM activ
        WHERE activ_id = {0} AND NOT is_deleted
        LIMIT 1";

    public async Task<ActivEntity?> GetByIdAsync(int id) =>
        await QuerySingleAsync(string.Format(SelectQuery, "@id"), id).ConfigureAwait(false);

    public Task<IEnumerable<ActivEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = $@"SELECT activ_id,
                            usr_id,
                            org_id,
                            status_id,
                            visit_date,
                            start_time,
                            end_time,
                            description,
                            is_deleted
                    FROM activ
                    {(includeDeleted ? "" : "WHERE NOT is_deleted")}
                    LIMIT @pageSize OFFSET @offset";

        return QueryAsync(sql, new { pageSize, offset = (page - 1) * pageSize });
    }

    public Task<int> CreateAsync(ActivEntity activ)
    {
        const string sql = @"INSERT INTO activ (usr_id, org_id, status_id, visit_date, start_time, end_time, description)
                            VALUES (@usr_id, @org_id, @status_id, @visit_date, @start_time, @end_time, @description)
                            RETURNING activ_id";

        return ExecuteScalarAsync(sql, activ);
    }

    public async Task<bool> UpdateAsync(ActivEntity activ)
    {
        var existing = await GetByIdAsync(activ.activ_id);

        if (existing == null) return false;

        var updated = new ActivEntity(
            activ_id: existing.activ_id,
            usr_id: activ.usr_id ?? existing.usr_id,
            org_id: activ.org_id ?? existing.org_id,
            status_id: activ.status_id ?? existing.status_id,
            visit_date: activ.visit_date ?? existing.visit_date,
            start_time: activ.start_time ?? existing.start_time,
            end_time: activ.end_time ?? existing.end_time,
            description: activ.description ?? existing.description
        );

        const string sql = @"
                UPDATE activ
                SET usr_id = @usr_id,
                    org_id = @org_id,
                    status_id = @status_id,
                    visit_date = @visit_date,
                    start_time = @start_time,
                    end_time = @end_time,
                    description = @description
                WHERE activ_id = @activ_id";

        return await ExecuteAsync(sql, updated);
    }

    public Task<bool> HardDeleteAsync(int id) =>
        ExecuteAsync("DELETE FROM activ WHERE activ_id = @id", new { id });

    public Task<bool> SoftDeleteAsync(int id) =>
        ExecuteAsync("UPDATE activ SET is_deleted = true WHERE activ_id = @id", new { id });
}