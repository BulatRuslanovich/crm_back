namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/plan")]
public class PlanController(IPlanService planService)
: BaseApiController<ReadPlanDto, CreatePlanDto, UpdatePlanDto>(planService)
{ }
