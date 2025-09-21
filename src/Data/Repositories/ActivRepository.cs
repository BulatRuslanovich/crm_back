namespace CrmBack.Data.Repositories;

using Dapper;
using System.Data;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;

public class ActivRepository(IDbConnection dbConnection) : IActivRepository
{
    public async Task<ActivEntity?> GetByIdAsync(int id)
    {

        var sql = @"SELECT activ_id, usr_id, org_id, status_id, visit_date, start_time, end_time, description, created_at, updated_at, created_by, updated_by, is_deleted
                    FROM activ
                    WHERE activ_id = @activ_id AND NOT is_deleted
                    LIMIT 1";

        return await dbConnection.QuerySingleOrDefaultAsync<ActivEntity>(sql, new { activ_id = id }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ActivEntity>> GetAllAsync(bool includeDeleted = false)
    {
        var sql = @"SELECT activ_id, usr_id, org_id, status_id, visit_date, start_time, end_time, description, created_at, updated_at, created_by, updated_by, is_deleted
                    FROM activ";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        return await dbConnection.QueryAsync<ActivEntity>(sql).ConfigureAwait(false);
    }

    public async Task<int> CreateAsync(ActivEntity activ)
    {
        const string sql = @"INSERT INTO activ (usr_id, org_id, status_id, visit_date, start_time, end_time, description, created_at, updated_at) VALUES 
                            (@usr_id, 
                            @org_id, 
                            @status_id, 
                            @visit_date, 
                            @start_time, 
                            @end_time, 
                            @description, 
                            'system', 
                            'system');
                            RETURNING activ_id";

        return await dbConnection.ExecuteScalarAsync<int>(sql, activ).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(ActivEntity activ)
    {
        var sql = @"UPDATE activ
                    SET usr_id = @usr_id, 
                        org_id = @org_id, 
                        status_id = @status_id,
                        visit_date = @visit_date,
                        start_time = @start_time,
                        end_time = @end_time,
                        description = @description
                        updated_at = @updated_at
                    WHERE activ_id = @activ_id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, activ).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int id)
    {
        const string sql = "DELETE FROM activ WHERE activ_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"UPDATE activ 
                    SET is_deleted = true
                    WHERE activ_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }
}
