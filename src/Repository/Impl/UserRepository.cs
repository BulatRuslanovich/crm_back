namespace CrmBack.Repository.Impl;

using System.Data;
using CrmBack.Core.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;

public class UserRepository(IDbConnection dbConnection, IDistributedCache cache) 
    : BaseRepository<UserEntity, int>(dbConnection, cache), IUserRepository { }
