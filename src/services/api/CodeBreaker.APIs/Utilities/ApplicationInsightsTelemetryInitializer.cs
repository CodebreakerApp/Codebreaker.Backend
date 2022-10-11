namespace CodeBreaker.APIs.Utilities;

public class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker APIv2";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
