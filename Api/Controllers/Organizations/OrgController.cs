namespace CrmBack.Api.Controllers.Organizations;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Application.Organizations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Organizations (Org) management controller
/// Provides CRUD operations for organizations via CrudController base class
/// Security: Requires authentication (all endpoints are protected)
/// </summary>
[ApiVersion("1.0")]
[Authorize]
public class OrgController(IOrgService orgService, ILogger<OrgController> logger) : CrudController<ReadOrgDto, CreateOrgDto, UpdateOrgDto>(orgService, logger)
{
	/// <summary>
	/// Implements abstract method from CrudController
	/// Extracts organization ID from read DTO
	/// </summary>
	protected override int GetId(ReadOrgDto payload) => payload.OrgId;
}
