using System.Runtime.Serialization;

namespace CodeBreaker.Shared.Exceptions;

public class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException()
    {
    }

    public ConfigurationNotFoundException(string configurationKey)
    {
        ConfigurationKey = configurationKey;
    }

    public string ConfigurationKey { get; init; } = string.Empty;
}
