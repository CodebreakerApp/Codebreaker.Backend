

namespace CodeBreaker.APIs;

public class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker API";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
