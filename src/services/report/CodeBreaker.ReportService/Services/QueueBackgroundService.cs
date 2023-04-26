using CodeBreaker.Data.Common.Exceptions;
using CodeBreaker.Data.ReportService.Repositories;
using CodeBreaker.Queuing.Common.Exceptions;
using CodeBreaker.Queuing.Common.Models;
using CodeBreaker.Queuing.ReportService.Services;
using Mapster;

namespace CodeBreaker.ReportService.Services;

public class QueueBackgroundService : BackgroundService
{
    private static readonly TimeSpan[] _retryTimeouts = new TimeSpan[] {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromMinutes(1),
    };

    private static readonly ushort _retryReuse = 10;

    private readonly IServiceScope _serviceScope;

    private readonly ILogger _logger;

    private readonly IGameQueueReceiverService _gameQueueService;

    private readonly IGameRepository _gameRepository;

    public QueueBackgroundService(ILogger<QueueBackgroundService> logger, IServiceProvider services)
    {
        _logger = logger;
        _serviceScope = services.CreateScope();
        var scopedServices = _serviceScope.ServiceProvider;
        _gameQueueService = scopedServices.GetRequiredService<IGameQueueReceiverService>();
        _gameRepository = scopedServices.GetRequiredService<IGameRepository>();
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.BackgroundServiceStarting(nameof(QueueBackgroundService));
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        QueueMessage<Queuing.ReportService.Transfer.Game>? queueMessage;
        ulong retryCount = 0;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                queueMessage = await _gameQueueService.DequeueMessageAsync(cancellationToken);
            }
            catch (NoItemException)
            {
                _logger.NoItemInTheQueue();
                retryCount = await WaitAndIncrement(retryCount);
                continue;
            }

            Data.ReportService.Models.Game dbGame = queueMessage.Body.Adapt<Data.ReportService.Models.Game>();

            try
            {
                await _gameRepository.CreateAsync(dbGame, cancellationToken);
                await _gameQueueService.RemoveMessageAsync(queueMessage); // Remove the message from the queue, since it was successfully processed.
            }
            catch (CreateException ex)
            {
                _logger.LogError(ex, "Error storing the game with the id {id} to the database", queueMessage.Id);
            }

            retryCount = 0;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.BackgroundServiceStopping(nameof(QueueBackgroundService));
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _serviceScope.Dispose();
        base.Dispose();
    }

    private async Task<ulong> WaitAndIncrement(ulong retryCount)
    {
        if (_retryReuse == 0)
            throw new ArgumentOutOfRangeException(nameof(_retryReuse), "Must not be 0");

        ulong index = retryCount / _retryReuse;

        if (index == int.MaxValue || index == (ulong)_retryTimeouts.Length)
            index = (ulong)_retryTimeouts.Length - 1;

        await Task.Delay(_retryTimeouts[index]);

        if (retryCount != ulong.MaxValue)
            retryCount++;

        return retryCount;
    }
}
