namespace CrmBack.Repository.Impl;

using System.Data;
using CrmBack.Core.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;

public class OrgRepository(IDbConnection dbConnection, IDistributedCache cache)
    : BaseRepository<OrgEntity, int>(dbConnection, cache), IOrgRepository
{ }
