namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Data;

public class ActivRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<ActivEntity, int>(dbConnection, cache), IActivRepository { }