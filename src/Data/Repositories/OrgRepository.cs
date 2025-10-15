namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class OrgRepository(IDbConnection dbConnection) : BaseRepository<OrgEntity>(dbConnection), IOrgRepository
{
    private const string SelectQuery = @"
        SELECT org_id,
               name,
               inn,
               latitude,
               longitude,
               address,
               is_deleted
        FROM org
        WHERE org_id = {0} AND NOT is_deleted
        LIMIT 1";


    public Task<OrgEntity?> GetByIdAsync(int id) =>
        QuerySingleAsync(string.Format(SelectQuery, "@id"), id);


    public Task<IEnumerable<OrgEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = $@"SELECT org_id,
                            name,
                            inn,
                            latitude,
                            longitude,
                            address,
                            is_deleted
                    FROM org
                    {(includeDeleted ? "" : "WHERE NOT is_deleted")}
                    LIMIT @pageSize OFFSET @offset";

        return QueryAsync(sql, new { pageSize, offset = (page - 1) * pageSize });
    }

    public Task<int> CreateAsync(OrgEntity org)
    {
        const string sql = @"INSERT INTO org (name, inn, latitude, longitude, address)
                            VALUES (@name, @inn, @latitude, @longitude, @address)
                            RETURNING org_id";

        return ExecuteScalarAsync(sql, org);
    }

    public async Task<bool> UpdateAsync(OrgEntity org)
    {
        var existing = await GetByIdAsync(org.org_id);

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
        return await ExecuteAsync(sql, updated);
    }

    public Task<bool> HardDeleteAsync(int id) =>
        ExecuteAsync("DELETE FROM org WHERE org_id = @id", new { id });

    public Task<bool> SoftDeleteAsync(int id) =>
        ExecuteAsync("UPDATE org SET is_deleted = true WHERE org_id = @id", new { id });
}