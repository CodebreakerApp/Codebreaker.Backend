using CodeBreaker.LiveService.Options;
using Microsoft.AspNetCore.SignalR;

namespace CodeBreaker.LiveService.Extensions;

public static class ApplicationBuilderExtensions
{
    public static ISignalRServerBuilder AddAzureSignalRIfConfigured(this ISignalRServerBuilder signalRServerBuilder, LiveServiceOptions options)
    {
        if (string.IsNullOrEmpty(options.ConnectionStrings.SignalR))
            return signalRServerBuilder;

        return signalRServerBuilder.AddAzureSignalR(options.ConnectionStrings.SignalR);
    }
}
