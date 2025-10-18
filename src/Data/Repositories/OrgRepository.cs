namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class OrgRepository(IDbConnection dbConnection) : Repository<OrgEntity, int>(
    dbConnection,
    tableName: "org",
    keyColumn: "org_id",
    columns: ["org_id", "name", "inn", "latitude", "longitude", "address", "is_deleted"],
    insertColumns: ["name", "inn", "latitude", "longitude", "address"],
    updateColumns: ["name", "inn", "latitude", "longitude", "address", "is_deleted"]
), IOrgRepository
{ }