namespace CodeBreaker.LiveService.Options;

public class LiveServiceOptions
{
    public ConnectionStringsOptions ConnectionStrings { get; set; } = new ConnectionStringsOptions();

    public EventHubOptions EventHub { get; set; } = new EventHubOptions();
}

public class ConnectionStringsOptions
{
    public string? SignalR { get; set; }
}

public class EventHubOptions
{
    public string? ConsumerGroupName { get; set; }

    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
