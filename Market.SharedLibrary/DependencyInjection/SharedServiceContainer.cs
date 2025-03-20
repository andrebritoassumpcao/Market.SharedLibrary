using Market.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Market.SharedLibrary.DependencyInjection;
public static class SharedServiceContainer
{
    public static IServiceCollection AddSharedServices<TContext>
        (this IServiceCollection services, IConfiguration configuration, string fileName) where TContext : DbContext
    {
        services.AddDbContext<TContext>(option => option.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"), sqlServerOption =>
            sqlServerOption.EnableRetryOnFailure()
            ));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.File(path: $"{fileName}-.txt",
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day)
            .CreateLogger();

        JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, configuration);
        return services;
    }
    public static IApplicationBuilder UseSharedPolices(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalException>();

        app.UseMiddleware<ListenToOnlyApiGateway>();

        return app;
    }
}
