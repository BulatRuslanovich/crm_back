namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    // read
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReadUserPayload>> GetById(int id)
    {
        try
        {
            var user = await userService.GetUserById(id).ConfigureAwait(true);
            return Ok(user);
        }
        catch (NullReferenceException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    // read all
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadUserPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ReadUserPayload>>> GetAllUsers()
    {
        try
        {
            var users = await userService.GetAllUsers().ConfigureAwait(true);
            return Ok(users);
        }
        catch (NullReferenceException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }



    // create
    [HttpPost]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadUserPayload>> Create([FromBody] CreateUserPayload user)
    {
        try
        {
            var userDTO = await userService.CreateUser(user).ConfigureAwait(false);
            var payload = userDTO.ToReadPayload();
           
            return CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


}