using Codebreaker.Identity.Configuration;
using Codebreaker.Identity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Codebreaker.Identity.Extensions;

/// <summary>
/// Extension methods to configure Codebreaker.Identity services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the anonymous user service to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAnonymousUserService(this IServiceCollection services)
    {
        services.AddScoped<IAnonymousUserService, GraphAnonymousUserService>();
        return services;
    }
    
    /// <summary>
    /// Adds the anonymous user service to the service collection with configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">The configuration action</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAnonymousUserService(
        this IServiceCollection services,
        Action<AnonymousUserOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<IAnonymousUserService, GraphAnonymousUserService>();
        return services;
    }
}