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

public class ActivRepository(IDbConnection dbConnection,
 ILogger<ActivRepository> logger,
  IOptions<DatabaseLoggingOptions> loggingOptions) : BaseRepository<ActivEntity>(dbConnection, logger, loggingOptions), IActivRepository
{
    private readonly IDbConnection dbConnection = dbConnection;
    private readonly bool enableDbLog = loggingOptions.Value.EnableDatabaseLogging;

    public async Task<ActivEntity?> GetByIdAsync(int id)
    {
        return await GetByIdAsync(id, null);
    }

    private async Task<ActivEntity?> GetByIdAsync(int id, IDbTransaction? transaction = null)
    {
        var sql = @"SELECT activ_id,
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
                    WHERE activ_id = @id AND NOT is_deleted
                    LIMIT 1";
        return await GetByIdAsync(sql, id, transaction).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ActivEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = @"SELECT activ_id,
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
                    FROM activ";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        sql += " LIMIT @PageSize OFFSET @Offset";

        return await GetAllAsync(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize }).ConfigureAwait(false);
    }

    public async Task<int> CreateAsync(ActivEntity activ)
    {
        const string sql = @"INSERT INTO activ (usr_id, org_id, status_id, visit_date, start_time, end_time, description, created_by, updated_by)
                            VALUES (@usr_id, @org_id, @status_id, @visit_date, @start_time, @end_time, @description, 'system', 'system')
                            RETURNING activ_id";

        return await CreateAsync(sql, activ).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(ActivEntity activ)
    {
        using var tran = dbConnection.BeginTransaction();

        try
        {
            var oldActiv = await GetByIdAsync(activ.activ_id, tran);

            if (oldActiv == null)
            {
                if (enableDbLog)
                {
                    logger.LogDebug("Activity with ID {Id} not found for update", activ.activ_id);
                }
                tran.Rollback();
                return false;
            }

            var result = new ActivEntity(
                activ_id: activ.activ_id,
                usr_id: activ.usr_id ?? oldActiv.usr_id,
                org_id: activ.org_id ?? oldActiv.org_id,
                status_id: activ.status_id ?? oldActiv.status_id,
                visit_date: activ.visit_date ?? oldActiv.visit_date,
                start_time: activ.start_time ?? oldActiv.start_time,
                end_time: activ.end_time ?? oldActiv.end_time,
                description: activ.description == "-" ? oldActiv.description : activ.description
            );

            var sql = @"UPDATE activ
                        SET usr_id = @usr_id,
                            org_id = @org_id,
                            status_id = @status_id,
                            visit_date = @visit_date,
                            start_time = @start_time,
                            end_time = @end_time,
                            description = @description
                        WHERE activ_id = @activ_id";

            LogSql(sql, result);

            var affectedRows = await dbConnection.ExecuteAsync(sql, result, tran).ConfigureAwait(false);

            if (enableDbLog)
            {
                logger.LogDebug("Updated activity with ID {Id}, affected rows: {AffectedRows}", activ.activ_id, affectedRows);
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
        const string sql = "DELETE FROM activ WHERE activ_id = @Id";
        return await DeleteAsync(sql, new { Id = id }, isHardDelete: true).ConfigureAwait(false);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"UPDATE activ 
                    SET is_deleted = true
                    WHERE activ_id = @Id";
        return await DeleteAsync(sql, new { Id = id }, isHardDelete: false).ConfigureAwait(false);
    }
}