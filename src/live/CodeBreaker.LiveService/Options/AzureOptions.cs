namespace CodeBreaker.LiveService.Options;

public class AzureOptions
{
    public const string Name = "Azure";

    public SignalROptions? SignalR { get; set; }

    public EventHubOptions EventHub { get; set; } = new EventHubOptions();
}

public class SignalROptions
{
    public string? ConnectionString { get; set; }
}

public class EventHubOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string? ConsumerGroupName { get; set; }

    public string? Name { get; set; }
}
