namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Logging;

public class ActivRepository(IDbConnection dbConnection,
 ILogger<ActivRepository> logger) : BaseRepository<ActivEntity>(dbConnection, logger), IActivRepository
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
               created_at,
               updated_at,
               created_by,
               updated_by,
               is_deleted
        FROM activ
        WHERE {0} AND NOT is_deleted
        LIMIT 1";

    public async Task<ActivEntity?> GetByIdAsync(int id) =>
        await QuerySingleAsync(string.Format(SelectQuery, "activ_id = @id"), id).ConfigureAwait(false);

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
                            created_at,
                            updated_at,
                            created_by,
                            updated_by,
                            is_deleted
                    FROM activ
                    {(includeDeleted ? "" : "WHERE NOT is_deleted")}
                    LIMIT @PageSize OFFSET @Offset";

        return QueryAsync(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    public Task<int> CreateAsync(ActivEntity activ)
    {
        const string sql = @"INSERT INTO activ (usr_id, org_id, status_id, visit_date, start_time, end_time, description, created_by, updated_by)
                            VALUES (@usr_id, @org_id, @status_id, @visit_date, @start_time, @end_time, @description, 'system', 'system')
                            RETURNING activ_id";

        return ExecuteScalarAsync(sql, activ);
    }

    public Task<bool> UpdateAsync(ActivEntity activ) =>
        WithTransactionAsync(async transaction =>
        {
            var existing = await QuerySingleAsync(
                string.Format(SelectQuery, "activ_id = @id"), activ.activ_id, transaction).ConfigureAwait(false);

            if (existing == null) return false;

            var updated = new ActivEntity(
                activ_id: existing.activ_id,
                usr_id: activ.usr_id ?? existing.usr_id,
                org_id: activ.org_id ?? existing.org_id,
                status_id: activ.status_id ?? existing.status_id,
                visit_date: activ.visit_date ?? existing.visit_date,
                start_time: activ.start_time ?? existing.start_time,
                end_time: activ.end_time ?? existing.end_time,
                description: activ.description == "-" ? existing.description : activ.description
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

            return await ExecuteAsync(sql, updated, transaction);
        });
    public Task<bool> HardDeleteAsync(int id) =>
        ExecuteAsync("DELETE FROM activ WHERE activ_id = @Id", new { Id = id });

    public Task<bool> SoftDeleteAsync(int id) =>
        ExecuteAsync("UPDATE activ SET is_deleted = true WHERE activ_id = @Id", new { Id = id });
}
