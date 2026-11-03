using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CourseWebApi.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
    }
}
