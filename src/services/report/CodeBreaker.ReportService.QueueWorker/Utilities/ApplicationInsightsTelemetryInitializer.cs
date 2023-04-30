using Microsoft.ApplicationInsights.Channel;

namespace Microsoft.ApplicationInsights.Extensibility;

internal class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker Report - QueueWorker";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
