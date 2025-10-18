namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Data;

public class PlanRepository(IDbConnection dbConnection) : Repository<PlanEntity, int>(dbConnection), IPlanRepository { }