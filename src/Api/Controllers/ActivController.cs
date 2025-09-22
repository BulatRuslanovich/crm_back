namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/activ")]
public class ActivController(IActivService activService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadActivPayload>> GetById(int id)
    {
        try
        {
            var activ = await activService.GetActivById(id).ConfigureAwait(true);
            return activ != null ? Ok(activ) : NotFound();
        }

        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<ReadActivPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadActivPayload>>> GetAll()
    {
        try
        {
            var users = await activService.GetAllActiv().ConfigureAwait(true);

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
    [Authorize]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadActivPayload>> Create([FromBody] CreateActivPayload activ)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var payload = await activService.CreateActiv(activ).ConfigureAwait(false);
            return payload != null ? CreatedAtAction(nameof(GetById), new { id = payload.ActivId }, payload) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateActivPayload payload)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await activService.UpdateActiv(id, payload).ConfigureAwait(true);
            return updated ? Ok(updated) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await activService.DeleteActiv(id).ConfigureAwait(true);
        return deleted ? NoContent() : NotFound();
    }
}

