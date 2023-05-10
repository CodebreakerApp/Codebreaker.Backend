using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Codebreaker.APIs.Extensions;

public static class OpenTelemetryExtensions
{
    const string ServiceName = "Codebreaker.GameAPI";
    const string ServiceVersion = "2.0.1";

    private static ResourceBuilder ResourceBuilder { get; } =
        ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);

    public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddConsoleExporter()
                    .AddSource(ServiceName, ServiceVersion)
                    .SetResourceBuilder(ResourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation();
            });
        return services;
    }

    public static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddConsoleExporter()
                    .AddMeter(GameEndpoints.Meter.Name)
                    .SetResourceBuilder(ResourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();
            });
        return services;
    }

    public static void AddOpenTelemetryLogging(this ILoggingBuilder logger)
    {       
        logger.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder)
            .AddConsoleExporter();
        });
    }
}
