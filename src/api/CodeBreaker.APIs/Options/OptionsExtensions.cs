namespace CodeBreaker.APIs.Options;

public static class OptionsExtensions
{
    public static void BindOptions<TOptions>(this IServiceCollection services, IConfiguration configuration)
        where TOptions : class, IAutoBindableOptions, new()
    {
        string sectionKey = new TOptions().GetSectionKey();
        services.AddOptions<TOptions>().Bind(configuration.GetSection(sectionKey));
    }
}
