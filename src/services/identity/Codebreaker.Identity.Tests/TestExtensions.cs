using Codebreaker.Identity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Codebreaker.Identity.Tests;

/// <summary>
/// Extension methods for testing
/// </summary>
public static class TestExtensions
{
    /// <summary>
    /// Adds a mock anonymous user service for testing purposes
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMockAnonymousUserService(this IServiceCollection services)
    {
        services.AddScoped<IAnonymousUserService, MockAnonymousUserService>();
        return services;
    }
}