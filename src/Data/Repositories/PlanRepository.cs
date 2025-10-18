namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class PlanRepository(IDbConnection dbConnection) : Repository<PlanEntity, int>(
    dbConnection,
    tableName: "plan",
    keyColumn: "plan_id",
    columns: ["plan_id", "usr_id", "org_id", "start_date", "end_date", "is_deleted"],
    insertColumns: ["usr_id", "org_id", "start_date", "end_date"],
    updateColumns: ["usr_id", "org_id", "start_date", "end_date", "is_deleted"]
), IPlanRepository
{ }