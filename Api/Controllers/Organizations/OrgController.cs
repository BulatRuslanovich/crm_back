namespace CrmBack.Api.Controllers.Organizations;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Application.Organizations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
[Authorize]
public class OrgController(IOrgService orgService, ILogger<OrgController> logger) : CrudController<ReadOrgDto, CreateOrgDto, UpdateOrgDto>(orgService, logger)
{

    protected override int GetId(ReadOrgDto payload) => payload.OrgId;
}
