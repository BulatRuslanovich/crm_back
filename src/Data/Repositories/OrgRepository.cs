namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Data;

public class OrgRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<OrgEntity, int>(dbConnection, cache), IOrgRepository { }