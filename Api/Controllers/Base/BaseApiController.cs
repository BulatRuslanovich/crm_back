namespace CrmBack.Api.Controllers.Base;

using System;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
	protected void ValidateId(int id)
	{
		if (id <= 0)
		{
			throw new ArgumentException("ID must be greater than 0", nameof(id));
		}
	}
}
