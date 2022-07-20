using CodeBreaker.LiveService.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;

namespace CodeBreaker.LiveService.Extensions;

public static class ApplicationBuilderExtensions
{
    public static ISignalRServerBuilder AddAzureSignalRIfConfigured(this ISignalRServerBuilder signalRServerBuilder, IConfiguration configuration, Action<ServiceOptions> options)
    {
        AzureOptions azureOptions = new AzureOptions();
        configuration.GetSection(AzureOptions.Name).Bind(azureOptions);

        if (string.IsNullOrEmpty(azureOptions?.SignalR?.ConnectionString))
            return signalRServerBuilder;

        return signalRServerBuilder.AddAzureSignalR(options);
    }

    public static ISignalRServerBuilder AddAzureSignalRIfConfigured(this ISignalRServerBuilder signalRServerBuilder, IConfiguration configuration)
    {
        AzureOptions azureOptions = new AzureOptions();
        configuration.GetSection(AzureOptions.Name).Bind(azureOptions);

        if (string.IsNullOrEmpty(azureOptions?.SignalR?.ConnectionString))
            return signalRServerBuilder;

        return signalRServerBuilder.AddAzureSignalR();
    }
}
