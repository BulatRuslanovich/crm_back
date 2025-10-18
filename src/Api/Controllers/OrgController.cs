
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/org")]
public class OrgController(IOrgService orgService, IDistributedCache cache) 
: BaseApiController<ReadOrgPayload, CreateOrgPayload, UpdateOrgPayload>(cache, "org_", orgService) { }