namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class OrgRepository(IDbConnection dbConnection) : BaseRepository<OrgEntity>(dbConnection), IOrgRepository
{
    private const string OrgColumns = "org_id, name, inn, latitude, longitude, address, is_deleted";

    private const string SelectByIdSql = $@"
        SELECT {OrgColumns}
        FROM org
        WHERE org_id = @id AND NOT is_deleted
        LIMIT 1";


    public Task<OrgEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        QuerySingleAsync(SelectByIdSql, id, ct);


    public Task<IEnumerable<OrgEntity>> GetAllAsync(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var where = isDeleted ? "" : "WHERE NOT is_deleted";
        var sql = $@"SELECT {OrgColumns}
                    FROM org
                    {where}
                    LIMIT @pageSize OFFSET @offset";

        return QueryAsync(sql, new { pageSize, offset = (page - 1) * pageSize }, ct);
    }

    public Task<int> CreateAsync(OrgEntity org, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO org (name, inn, latitude, longitude, address)
                            VALUES (@name, @inn, @latitude, @longitude, @address)
                            RETURNING org_id";

        return ExecuteScalarAsync(sql, org, ct);
    }

    public async Task<bool> UpdateAsync(OrgEntity org, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(org.org_id, ct);

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
        return await ExecuteAsync(sql, updated, ct);
    }

    public Task<bool> HardDeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync("DELETE FROM org WHERE org_id = @id", new { id }, ct);

    public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync("UPDATE org SET is_deleted = true WHERE org_id = @id", new { id }, ct);
}