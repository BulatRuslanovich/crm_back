namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class ActivRepository(IDbConnection dbConnection) : Repository<ActivEntity, int>(
    dbConnection,
    tableName: "activ",
    keyColumn: "activ_id",
    columns: ["activ_id", "usr_id", "org_id", "status_id", "visit_date", "start_time", "end_time", "description", "is_deleted"],
    insertColumns: ["usr_id", "org_id", "status_id", "visit_date", "start_time", "end_time", "description"],
    updateColumns: ["usr_id", "org_id", "status_id", "visit_date", "start_time", "end_time", "description", "is_deleted"]
), IActivRepository
{ }