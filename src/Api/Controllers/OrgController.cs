
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/org")]
public class OrgController(IOrgService orgService)
: BaseApiController<ReadOrgPayload, CreateOrgPayload, UpdateOrgPayload>(orgService)
{ }