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
    private const string SelectQuery = @"
        SELECT org_id,
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
        WHERE {0} AND NOT is_deleted
        LIMIT 1";


    public Task<OrgEntity?> GetByIdAsync(int id) =>
        QuerySingleAsync(string.Format(SelectQuery, "org_id = @id"), id);


    public Task<IEnumerable<OrgEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = $@"SELECT org_id,
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
                    {(includeDeleted ? "" : "WHERE NOT is_deleted")}
                    LIMIT @PageSize OFFSET @Offset
                    ";

        return QueryAsync(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    public Task<int> CreateAsync(OrgEntity org)
    {
        const string sql = @"INSERT INTO org (name, inn, latitude, longitude, address, created_by, updated_by)
                            VALUES (@name, @inn, @latitude, @longitude, @address, 'system', 'system')
                            RETURNING org_id";

        return ExecuteScalarAsync(sql, org);
    }

    public Task<bool> UpdateAsync(OrgEntity org) =>
    WithTransactionAsync(async transaction =>
    {
        var existing = await QuerySingleAsync(
            string.Format(SelectQuery, "org_id = @id"), org.org_id, transaction);

        if (existing == null) return false;

        var updated = new OrgEntity(
            org_id: existing.org_id,
            name: org.name ?? existing.name,
            inn: org.inn ?? existing.inn,
            latitude: org.latitude ?? existing.latitude,
            longitude: org.longitude ?? existing.longitude,
            address: org.address ?? existing.address
        );

        const string sql = @"UPDATE org 
                            SET name = @name,
                                inn = @inn,
                                latitude = @latitude,
                                longitude = @longitude,
                                address = @address
                            WHERE org_id = @org_id";
        return await ExecuteAsync(sql, updated, transaction);
    });

    public Task<bool> HardDeleteAsync(int id) =>
    ExecuteAsync("DELETE FROM org WHERE org_id = @Id", new { Id = id });

    public Task<bool> SoftDeleteAsync(int id) =>
    ExecuteAsync("UPDATE org SET is_deleted = true WHERE org_id = @Id", new { Id = id });
}
