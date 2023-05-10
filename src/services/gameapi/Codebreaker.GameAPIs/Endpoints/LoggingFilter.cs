using Codebreaker.APIs;

using Microsoft.AspNetCore.Http.Extensions;

namespace Codebreaker.GameAPIs.Endpoints;

public class LoggingFilter : IEndpointFilter
{
    private readonly ILogger _logger;
    public LoggingFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoggingFilter>();
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        _logger.ApiRequest(context.HttpContext.Request.GetDisplayUrl(), context.HttpContext.Request.Method);
        return await next(context);
    }
}
