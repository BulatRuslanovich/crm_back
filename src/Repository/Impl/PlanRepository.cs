namespace CrmBack.Repository.Impl;

using System.Data;
using CrmBack.Core.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;

public class PlanRepository(IDbConnection dbConnection, IDistributedCache cache) 
    : BaseRepository<PlanEntity, int>(dbConnection, cache), IPlanRepository { }
