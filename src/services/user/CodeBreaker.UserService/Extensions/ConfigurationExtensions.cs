namespace CodeBreaker.UserService.Extensions;

internal static class ConfigurationExtensions
{
    public static string GetRequired(this IConfiguration config, string key) =>
        config[key] ?? throw new InvalidOperationException($"Configuration key '{key}' is required.");
}
