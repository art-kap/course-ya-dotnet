using CourseWebApiProject.Dto;
using CourseWebApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourseWebApiProject.Controllers;

/// <summary>
/// Контроллер для обработки HTTP-запросов
/// </summary>
[ApiController]
[Route("bookings")]
public class BookingsController(IBookingService _bookingService) : ControllerBase
{
    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id брони</param>
    /// <response code="200">Возвращается в случае успешного ответа</response>
    /// <response code="404">Возвращается, если нет брони с данным id</response>
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(typeof(BookingInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<BookingInfo>> GetById([FromRoute] Guid id)
    {
        var bookingInfo = await _bookingService.GetBookingByIdAsync(id);
        return Ok(bookingInfo);
    }
}
