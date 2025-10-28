namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/org")]
public class OrgController(IOrgService orgService)
: BaseApiController<ReadOrgDto, CreateOrgDto, UpdateOrgDto>(orgService)
{ }
