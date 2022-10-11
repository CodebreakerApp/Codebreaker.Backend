using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace CodeBreaker.LiveService.Utilities;

public class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker Live (v2)";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
