using CourseWebApi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CourseWebApiProject.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        HttpStatusCode statusCode;
        string title, message;

        switch (exception)
        {
            case EntityNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Ресурс не найден.";
                message = exception.Message;
                _logger.LogWarning(exception, message);
                break;

            case EventValidationException:
                statusCode = HttpStatusCode.BadRequest;
                title = "Недопустимые данные.";
                message = exception.Message;
                _logger.LogWarning(exception, message);
                break;

            case EventHasBookingsException:
                statusCode = HttpStatusCode.Conflict;
                title = "Отказано в удалении события.";
                message = exception.Message;
                _logger.LogWarning(exception, message);
                break;

            case NoAvailableSeatsException:
                statusCode = HttpStatusCode.Conflict;
                title = "Отказано в бронировании.";
                message = exception.Message;
                _logger.LogWarning(exception, message);
                break;

            default:
                // По умолчанию возвращаем 500 ошибку
                statusCode = HttpStatusCode.InternalServerError;
                title = "Непредвиденная ошибка.";
                message = "Внутренняя ошибка сервера. Пожалуйста, попробуйте позже.";
                _logger.LogError(exception, "Ошибка 500 при выполнении запроса: {Path}", context.Request.Path);
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = message
        };

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        var jsonResponse = JsonSerializer.Serialize(response, options);
        return context.Response.WriteAsync(jsonResponse);
    }
}
