namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadUserPayload>> GetById(int id)
    {
        try
        {
            var user = await userService.GetUserById(id).ConfigureAwait(true);
            return user != null ? Ok(user) : NotFound();
        }

        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReadUserPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadUserPayload>>> GetAllUsers()
    {
        try
        {
            var users = await userService.GetAllUsers().ConfigureAwait(true);

            if (!users.Any())
            {
                return NotFound();
            }

            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadUserPayload>> Create([FromBody] CreateUserPayload user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var payload = await userService.CreateUser(user).ConfigureAwait(false);
            return payload != null ? CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateUser(int id, [FromBody] UpdateUserPayload payload)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await userService.UpdateUser(id, payload).ConfigureAwait(true);
            return updated ? Ok(updated) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // delete
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await userService.DeleteUser(id).ConfigureAwait(true);
        return deleted ? NoContent() : NotFound();
    }
}