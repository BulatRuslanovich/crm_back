using CrmBack.Domain.Activities;
using CrmBack.Domain.Auth;
using CrmBack.Domain.Organizations;
using CrmBack.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Data;

public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
{
	public DbSet<UserEntity> User { get; set; }
	public DbSet<ActivEntity> Activ { get; set; }
	public DbSet<OrgEntity> Org { get; set; }
	public DbSet<PolicyEntity> Policy { get; set; }
	public DbSet<StatusEntity> Status { get; set; }
	public DbSet<UserPolicyEntity> UserPolicies { get; set; }
	public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
}
