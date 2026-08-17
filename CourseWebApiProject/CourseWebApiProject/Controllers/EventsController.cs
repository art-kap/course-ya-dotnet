using CourseWebApiProject.Dto;
using CourseWebApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourseWebApiProject.Controllers;

/// <summary>
/// Контроллер для обработки HTTP-запросов
/// </summary>
[ApiController]
[Route("events")]
public class EventsController(IEventService _eventService): ControllerBase
{
    /// <summary>
    /// Получить список событий по фильтру
    /// </summary>
    /// <response code="200">Возвращается в случае успешного ответа</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public ActionResult<PaginatedResult> GetAll([FromQuery] EventsQuery query)
    {
        return Ok(_eventService.GetEventsByQuery(query));
    }

    /// <summary>
    /// Получить событие по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <response code="200">Возвращается в случае успешного ответа</response>
    /// <response code="404">Возвращается, если нет события с данным id</response>
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public ActionResult<EventResponseDto> GetById([FromRoute] Guid id)
    {
        return Ok(_eventService.GetEvent(id));
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    /// <param name="eventRequestDto">Данные события</param>
    /// <response code="201">Возвращается в случае успешного создания события</response>
    /// <response code="400">Возвращается, если входные данные некорректны</response>
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public IActionResult Post([FromBody] EventRequestDto eventRequestDto)
    {
        var responseDto = _eventService.AddEvent(eventRequestDto);
        return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
    }

    /// <summary>
    /// Обновить событие целиком
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="eventRequestDto">Данные события</param>
    /// <response code="204">Возвращается в случае успешного обновления события</response>
    /// <response code="400">Возвращается, если входные данные некорректны</response>
    /// <response code="404">Возвращается, если нет события с данным id</response>
    [HttpPut("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public IActionResult Put([FromRoute] Guid id, [FromBody] EventRequestDto eventRequestDto)
    {
        _eventService.UpdateEvent(id, eventRequestDto);
        return NoContent();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">id события</param>
    /// <response code="204">Возвращается в случае успешного удаления</response>
    /// <response code="404">Возвращается, если нет события с данным id</response>
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        _eventService.RemoveEvent(id);
        return NoContent();
    }
}
