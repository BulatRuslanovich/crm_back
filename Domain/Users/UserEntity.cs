using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrmBack.Application.Users.Dto;
using CrmBack.Domain.Activities;
using CrmBack.Domain.Common;

namespace CrmBack.Domain.Users;

[Table("usr")]
public class UserEntity : BaseEntity
{
	[Key]
	[Column("usr_id")]
	public int UsrId { get; set; }

	[Column("usr_firstname")]
	public string FirstName { get; set; } = string.Empty;

	[Column("usr_lastname")]
	public string LastName { get; set; } = string.Empty;

	[Column("usr_login")]
	public string Login { get; set; } = string.Empty;

	[Column("usr_pass")]
	public string PasswordHash { get; set; } = string.Empty;

	public virtual ICollection<ActivEntity> Activities { get; set; } = [];
	public virtual ICollection<UserPolicyEntity> UserPolicies { get; set; } = [];

	public void Update(UpdateUserDto dto)
	{
		FirstName = dto.FirstName ?? FirstName;
		LastName = dto.LastName ?? LastName;
		Login = dto.Login ?? Login;
	}
}
