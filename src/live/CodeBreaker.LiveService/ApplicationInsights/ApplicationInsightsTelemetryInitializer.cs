using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace CodeBreaker.LiveService.ApplicationInsights;

public class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker Live";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
