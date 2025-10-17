namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class ActivRepository(IDbConnection dbConnection) : BaseRepository<ActivEntity>(dbConnection), IActivRepository
{
    private const string ActivColumns = "activ_id, usr_id, org_id, status_id, visit_date, start_time, end_time, description, is_deleted";

    private const string SelectByIdSql = $@"
        SELECT {ActivColumns}
        FROM activ
        WHERE activ_id = @id AND NOT is_deleted
        LIMIT 1";

    public async Task<ActivEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await QuerySingleAsync(SelectByIdSql, id, ct).ConfigureAwait(false);

    public Task<IEnumerable<ActivEntity>> GetAllAsync(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var where = isDeleted ? "" : "WHERE NOT is_deleted";
        var sql = $@"SELECT {ActivColumns}
                    FROM activ
                    {where}
                    LIMIT @pageSize OFFSET @offset";

        return QueryAsync(sql, new { pageSize, offset = (page - 1) * pageSize }, ct);
    }


    public Task<int> CreateAsync(ActivEntity activ, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO activ (usr_id, org_id, status_id, visit_date, start_time, end_time, description)
                            VALUES (@usr_id, @org_id, @status_id, @visit_date, @start_time, @end_time, @description)
                            RETURNING activ_id";

        return ExecuteScalarAsync(sql, activ, ct);
    }

    public async Task<bool> UpdateAsync(ActivEntity activ, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(activ.activ_id, ct);

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

        return await ExecuteAsync(sql, updated, ct);
    }

    public Task<bool> HardDeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync("DELETE FROM activ WHERE activ_id = @id", new { id }, ct);

    public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync("UPDATE activ SET is_deleted = true WHERE activ_id = @id", new { id }, ct);
}