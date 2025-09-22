namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class OrgRepository(IDbConnection dbConnection, ILogger<OrgRepository> logger, IOptions<DatabaseLoggingOptions> loggingOptions) : BaseRepository<OrgEntity>(dbConnection, logger, loggingOptions), IOrgRepository
{
    private readonly IDbConnection dbConnection = dbConnection;
    private readonly bool enableDbLog = loggingOptions.Value.EnableDatabaseLogging;

    public async Task<OrgEntity?> GetByIdAsync(int id)
    {
        return await GetByIdAsync(id, null);
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
                    WHERE org_id = @id AND NOT is_deleted
                    LIMIT 1";
        return await GetByIdAsync(sql, id, transaction).ConfigureAwait(false);
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

        return await GetAllAsync(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize }).ConfigureAwait(false);
    }

    public async Task<int> CreateAsync(OrgEntity org)
    {
        const string sql = @"INSERT INTO org (name, inn, latitude, longitude, address, created_by, updated_by)
                            VALUES (@name, @inn, @latitude, @longitude, @address, 'system', 'system')
                            RETURNING org_id";

        return await CreateAsync(sql, org).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(OrgEntity org)
    {
        using var tran = dbConnection.BeginTransaction();

        try
        {
            var orgFromDb = await GetByIdAsync(org.org_id, tran);

            if (orgFromDb == null)
            {
                if (enableDbLog)
                {
                    logger.LogDebug("Organization with ID {Id} not found for update", org.org_id);
                }
                tran.Rollback();
                return false;
            }

            var result = new OrgEntity(
                org_id: orgFromDb.org_id,
                name: org.name ?? orgFromDb.name,
                inn: org.inn ?? orgFromDb.inn,
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

            LogSql(sql, result);

            var affectedRows = await dbConnection.ExecuteAsync(sql, result, tran).ConfigureAwait(false);

            if (enableDbLog)
            {
                logger.LogDebug("Updated organization with ID {Id}, affected rows: {AffectedRows}", org.org_id, affectedRows);
            }

            tran.Commit();
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
        return await DeleteAsync(sql, new { Id = id }, isHardDelete: true).ConfigureAwait(false);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"UPDATE org 
                    SET is_deleted = true
                    WHERE org_id = @Id";
        return await DeleteAsync(sql, new { Id = id }, isHardDelete: false).ConfigureAwait(false);
    }
}