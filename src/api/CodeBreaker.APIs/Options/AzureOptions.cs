namespace CodeBreaker.APIs.Options;

public class AzureOptions : IAutoBindableOptions
{
    public const string SectionKey = "Azure";

    public string GetSectionKey() => SectionKey;

    public EventHubOptions EventHub { get; set; } = new EventHubOptions();
}

public class EventHubOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string? ConsumerGroupName { get; set; }

    public string? Name { get; set; }
}
