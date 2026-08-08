using CourseWebApiProject.Dto;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace CourseWebApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService _eventService): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    public ActionResult<List<EventDto>> GetAll()
    {
        return Ok(_eventService.GetAllEvents());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventDto> GetById([FromRoute] int id)
    {
        return Ok(_eventService.GetEvent(id));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] EventDto eventDto)
    {
        _eventService.AddEvent(eventDto);
        return CreatedAtAction(nameof(GetById), new { id = eventDto.Id }, eventDto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put([FromRoute] int id, [FromBody] EventPutDto eventPutDto)
    {
        _eventService.UpdateEvent(eventPutDto.ToDto(id));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] int id)
    {
        _eventService.RemoveEvent(id);
        return Ok();
    }
}
