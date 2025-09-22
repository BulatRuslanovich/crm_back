namespace CrmBack.Data.Repositories;

using System.Data;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Dapper;

public class OrgRepository(IDbConnection dbConnection) : IOrgRepository
{
    public async Task<OrgEntity?> GetByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    private async Task<OrgEntity?> GetByIdAsync(int id, IDbTransaction? transaction = null)
    {

        var sql = @"SELECT org_id,
                           name,
                           inn,
                           latitude,
                           longitude,
                           address,
                           created_at,
                           updated_at,
                           created_by,
                           updated_by,
                           is_deleted
                    FROM org
                    WHERE org_id = ? AND NOT is_deleted;
                    LIMIT 1";

        return await dbConnection.QuerySingleOrDefaultAsync<OrgEntity>(sql, new { activ_id = id }, transaction).ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrgEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = @"SELECT org_id,
                           name,
                           inn,
                           latitude,
                           longitude,
                           address,
                           created_at,
                           updated_at,
                           created_by,
                           updated_by,
                           is_deleted
                    FROM org";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        sql += " LIMIT @PageSize OFFSET @Offset";

        return await dbConnection.QueryAsync<OrgEntity>(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize }).ConfigureAwait(false);
    }

    public async Task<int> CreateAsync(OrgEntity activ)
    {
        const string sql = @"INSERT INTO org (name, inn, latitude, longitude, address, created_by, updated_by) VALUES 
                            (@name, @inn, @latitude, @longitude, @address, 'system', 'system');
                            RETURNING org_id";

        return await dbConnection.ExecuteScalarAsync<int>(sql, activ).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(OrgEntity org)
    {
        using var tran = dbConnection.BeginTransaction();

        try
        {
            var orgFromDb = await GetByIdAsync(org.org_id, tran);

            if (orgFromDb == null)
            {
                return false;
            }

            var result = new OrgEntity(
                org_id: orgFromDb.org_id,
                name: org.name ?? orgFromDb.name,
                latitude: org.latitude ?? orgFromDb.latitude,
                longitude: org.longitude ?? orgFromDb.longitude,
                address: org.address ?? orgFromDb.address
            );

            var sql = @"UPDATE org 
                        SET name = @name, 
                            inn = @inn,
                            latitude = @latitude,
                            longitude = @longitude,
                            address = @address
                        WHERE org_id = @org_id";

            var affectedRows = await dbConnection.ExecuteAsync(sql, result, tran).ConfigureAwait(false);
            return affectedRows > 0;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> HardDeleteAsync(int id)
    {
        const string sql = "DELETE FROM org WHERE org_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"UPDATE org 
                    SET is_deleted = true
                    WHERE org_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }
}
