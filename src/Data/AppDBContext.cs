using CrmBack.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Data;

public class AppDBContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<UserEntity> User { get; set; }
    public DbSet<ActivEntity> Activ { get; set; }
    public DbSet<OrgEntity> Org { get; set; }
    public DbSet<PlanEntity> Plan { get; set; }
    public DbSet<PolicyEntity> Policy { get; set; }
    public DbSet<StatusEntity> Status { get; set; }
    public DbSet<UserPolicyEntity> UserPolicies { get; set; }
}
