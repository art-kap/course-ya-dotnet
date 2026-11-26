using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseWebApiProject.Controllers;

/// <summary>
/// Контроллер для обработки HTTP-запросов
/// </summary>
[Authorize]
[ApiController]
[Route("bookings")]
public class BookingsController(IBookingService _bookingService) : ControllerBase
{
    /// <summary>
    /// Получить бронь по id
    /// </summary>
    /// <param name="id">id брони</param>
    /// <response code="200">Возвращается в случае успешного ответа</response>
    /// <response code="401">Возвращается, если пользователь не вошел в систему</response>
    /// <response code="404">Возвращается, если нет брони с данным id</response>
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(typeof(BookingInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<BookingInfo>> GetById([FromRoute] Guid id)
    {
        var bookingInfo = await _bookingService.GetBookingByIdAsync(id);
        return Ok(bookingInfo);
    }

    /// <summary>
    /// Отменяет бронь по id
    /// </summary>
    /// <param name="id">id брони</param>
    /// <response code="204">Возвращается в случае успешной отмены</response>
    /// <response code="401">Возвращается, если пользователь не вошел в систему</response>
    /// <response code="403">Возвращается в случае отсутствия прав на удаление</response>
    /// <response code="404">Возвращается, если нет брони с данным id</response>
    /// <response code="409">Возвращается, если бронь с данным id была отменена ранее</response>
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingInfo>> Cancel([FromRoute] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return BadRequest("Идентификатор пользователя не найден");
        }

        await _bookingService.CancelBooking(id, Guid.Parse(userIdClaim.Value));
        return NoContent();
    }
}
