using CodeBreaker.Shared.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration;

public static class ConfigExtensions
{
    public static string GetRequired(this IConfiguration config, string configKey) =>
        config[configKey] ?? throw new ConfigurationNotFoundException(configKey);
}
