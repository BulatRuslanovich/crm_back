
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/plan")]
public class PlanController(IPlanService planService, IDistributedCache cache) 
: BaseApiController<ReadPlanPayload, CreatePlanPayload, UpdatePlanPayload>(cache, "plan_", planService) { }