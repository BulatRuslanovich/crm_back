namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/usr")]
public class UserController(IUserService userService) : BaseApiController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService)
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto Dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await userService.Login(Dto, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto Dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await userService.Create(Dto, HttpContext.RequestAborted);
            if (response == null) return BadRequest(new { message = "Failed to register user" });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("{id:int}/activ")]
    public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id)
    {
        var activs = await userService.GetActivs(id, HttpContext.RequestAborted);
        return Ok(activs);
    }
}
