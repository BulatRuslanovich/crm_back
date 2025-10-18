namespace CrmBack.Repository.Impl;

using System.Data;
using CrmBack.Core.Models.Entities;
using CrmBack.Repository;
using Microsoft.Extensions.Caching.Distributed;

public class ActivRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<ActivEntity, int>(dbConnection, cache), IActivRepository { }
