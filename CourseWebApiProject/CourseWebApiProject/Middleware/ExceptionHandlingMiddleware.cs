using CourseWebApiProject.Exceptions;
using System.Net;
using System.Text.Json;

namespace CourseWebApiProject.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // По умолчанию возвращаем 500 ошибку
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Внутренняя ошибка сервера. Пожалуйста, попробуйте позже.";

        // Здесь можно разделять логику для разных типов ваших кастомных исключений
        if (exception is EventNotFoundException)
        {
            statusCode = HttpStatusCode.NotFound;
            message = exception.Message;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}
