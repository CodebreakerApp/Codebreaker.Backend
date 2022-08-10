namespace CodeBreaker.LiveService;

public class EventHandlingService : BackgroundService
{
    private readonly IEventSourceService _eventSourceService;

    private readonly ILiveHubSender _liveHubSender;

    public EventHandlingService(IEventSourceService eventSourceService, ILiveHubSender liveHubSender)
    {
        _eventSourceService = eventSourceService;
        _liveHubSender = liveHubSender;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
            await foreach (var item in _eventSourceService.SubscribeAsync(token))
                await _liveHubSender.FireGameEvent(item, token);
    }
}
