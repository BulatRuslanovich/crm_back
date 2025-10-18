namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Data;

public class PlanRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<PlanEntity, int>(dbConnection, cache), IPlanRepository { }