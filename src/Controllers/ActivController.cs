
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Controllers;

[ApiController]
[Route("api/activ")]
public class ActivController(IActivService activService)
: BaseApiController<ReadActivPayload, CreateActivPayload, UpdateActivPayload>(activService)
{ }
