namespace CrmBack.Controllers;

using CrmBack.Core.Models.Payload.Org;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/org")]
public class OrgController(IOrgService orgService)
: BaseApiController<ReadOrgPayload, CreateOrgPayload, UpdateOrgPayload>(orgService) { }
