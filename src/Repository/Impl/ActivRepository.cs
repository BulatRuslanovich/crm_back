namespace CrmBack.Repository.Impl;

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Repository;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;

public class ActivRepository(IDbConnection dbConnection, IDistributedCache cache) 
    : BaseRepository<ActivEntity, int>(dbConnection, cache), IActivRepository {

    public async Task<IEnumerable<HumReadActivPayload>> GetAllHumActivsByUserIdAsync(
        int userId,
        CancellationToken ct = default)
    {
        var sql = $@"
            SELECT activ_id, usr_id, org.name as org_name, status.name as status_name, visit_date, start_time, end_time, description
            FROM activ
            JOIN org USING (org_id)
            JOIN status USING (status_id)
            WHERE NOT org.is_deleted
            AND NOT activ.is_deleted 
            AND NOT status.is_deleted
            AND activ.usr_id = @userId
            ORDER BY visit_date DESC, start_time DESC";

        var command = new CommandDefinition(sql, new { userId }, cancellationToken: ct);
        return await base.dbConnection.QueryAsync<HumReadActivPayload>(command).ConfigureAwait(false);
    }

    public async Task<IEnumerable<StatusEntity>> GetAllStatusAsync(CancellationToken ct = default)
    {
        var sql = $@"
            SELECT status_id, name, is_deleted
            FROM status
            WHERE NOT is_deleted";

        var command = new CommandDefinition(sql, cancellationToken: ct);
        return await base.dbConnection.QueryAsync<StatusEntity>(command).ConfigureAwait(false);
    }
}
