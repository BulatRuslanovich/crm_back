
using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivController(IActivService activService)
: BaseApiController<ReadActivDto, CreateActivDto, UpdateActivDto>(activService)
{ }
