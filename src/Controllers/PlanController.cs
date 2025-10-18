namespace CrmBack.Controllers;

using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/plan")]
public class PlanController(IPlanService planService)
: BaseApiController<ReadPlanPayload, CreatePlanPayload, UpdatePlanPayload>(planService)
{ }
