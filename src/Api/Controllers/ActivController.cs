
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/activ")]
public class ActivController(IActivService activService, IDistributedCache cache) 
: BaseApiController<ReadActivPayload, CreateActivPayload, UpdateActivPayload>(cache, "activ_", activService) { }