namespace CodeBreaker.APIs.Options;

public class ApiServiceOptions
{
    public EventHubOptions EventHub { get; set; } = new EventHubOptions();
}

public class EventHubOptions
{
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
