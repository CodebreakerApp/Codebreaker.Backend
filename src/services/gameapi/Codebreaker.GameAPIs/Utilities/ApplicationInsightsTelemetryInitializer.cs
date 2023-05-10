namespace Codebreaker.GameAPIs.Utilities;

public class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "CodeBreaker APIv3";
        telemetry.Context.Cloud.RoleInstance = "Azure Container App";
    }
}
