using Microsoft.ApplicationInsights.Channel;

namespace CodeBreaker.APIs;

public class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker Bot";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
