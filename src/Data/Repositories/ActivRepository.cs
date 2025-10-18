namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Data;

public class ActivRepository(IDbConnection dbConnection) : Repository<ActivEntity, int>(dbConnection), IActivRepository { }