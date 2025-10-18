
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/plan")]
public class PlanController(IPlanService planService)
: BaseApiController<ReadPlanPayload, CreatePlanPayload, UpdatePlanPayload>(planService)
{ }