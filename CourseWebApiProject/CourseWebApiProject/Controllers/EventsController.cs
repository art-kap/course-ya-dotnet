using CourseWebApiProject.Dto;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace CourseWebApiProject.Controllers;

/// <summary>
/// Контроллер для обработки HTTP-запросов
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService _eventService): ControllerBase
{
    /// <summary>
    /// Получить список всех событий
    /// </summary>
    /// <response code="200">Возвращается в случае успешного ответа</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public ActionResult<List<EventDto>> GetAll()
    {
        return Ok(_eventService.GetAllEvents());
    }

    /// <summary>
    /// Получить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <response code="200">Возвращается в случае успешного ответа</response>
    /// <response code="404">Возвращается, если нет события с данным id</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public ActionResult<EventDto> GetById([FromRoute] int id)
    {
        return Ok(_eventService.GetEvent(id));
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="eventDto">Данные события</param>
    /// <response code="201">Возвращается в случае успешного создания события</response>
    /// <response code="400">Возвращается, если входные данные некорректны</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public IActionResult Post([FromBody] EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (eventDto.StartAt >= eventDto.EndAt)
        {
            ModelState.AddModelError("EndAt", "Точное время окончания должно быть позже времени начала.");
            return BadRequest(ModelState);
        }

        _eventService.AddEvent(eventDto);
        return CreatedAtAction(nameof(GetById), new { id = eventDto.Id }, eventDto);
    }

    /// <summary>
    /// Обновить событие целиком
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="eventPutDto">Данные события</param>
    /// <response code="204">Возвращается в случае успешного обновления события</response>
    /// <response code="400">Возвращается, если входные данные некорректны</response>
    /// <response code="404">Возвращается, если нет события с данным id</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public IActionResult Put([FromRoute] int id, [FromBody] EventPutDto eventPutDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (eventPutDto.StartAt >= eventPutDto.EndAt)
        {
            ModelState.AddModelError("EndAt", "Точное время окончания должно быть позже времени начала.");
            return BadRequest(ModelState);
        }

        _eventService.UpdateEvent(eventPutDto.ToDto(id));
        return NoContent();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <response code="200">Возвращается в случае успешного удаления</response>
    /// <response code="404">Возвращается, если нет события с данным id</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public IActionResult Delete([FromRoute] int id)
    {
        _eventService.RemoveEvent(id);
        return Ok();
    }
}
