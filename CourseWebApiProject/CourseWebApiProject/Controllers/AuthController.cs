using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourseWebApiProject.Controllers;

/// <summary>
/// Контроллер для обработки HTTP-запросов
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController(IUserService _userService) : ControllerBase
{
    /// <summary>
    /// Создать пользователя
    /// </summary>
    /// <param name="userCreate">Данные пользователя</param>
    /// <response code="204">Возвращается в случае успешного создания пользователя</response>
    /// <response code="400">Возвращается, если данные некорректны</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<ActionResult> Register([FromBody] UserCreate userCreate)
    {
        await _userService.AddUser(userCreate);
        return NoContent();
    }

    /// <summary>
    /// Вход пользователя
    /// </summary>
    /// <param name="loginRequest">Информация для входа пользователя</param>
    /// <response code="200">Возвращается в случае успешного входа</response>
    /// <response code="400">Возвращается, если токен не выдан</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest loginRequest)
    {
        var token = await _userService.Login(loginRequest);
        return Ok(token);
    }
}
