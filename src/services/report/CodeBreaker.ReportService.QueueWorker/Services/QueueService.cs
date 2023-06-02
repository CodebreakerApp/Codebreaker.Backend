using System.Collections;
using CodeBreaker.Data.Common.Exceptions;
using CodeBreaker.Data.ReportService.Repositories;
using CodeBreaker.Queuing.Common.Exceptions;
using CodeBreaker.Queuing.Common.Models;
using CodeBreaker.Queuing.ReportService.Services;
using CodeBreaker.Queuing.ReportService.Transfer;
using CodeBreaker.ReportService.QueueWorker.Mapping;
using CodeBreaker.ReportService.QueueWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeBreaker.ReportService.QueueWorker.Services;

public class QueueService
{
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly ILogger _logger;

    private readonly IGameQueueReceiverService _gameQueueService;

    private readonly IGameRepository _gameRepository;

    private readonly RetryState _retryState;

    public QueueService(ILogger<QueueService> logger, IGameQueueReceiverService queueService, IGameRepository repository, IOptions<QueueServiceOptions> options)
    {
        _logger = logger;
        _gameQueueService = queueService;
        _gameRepository = repository;
        _retryState = new(options.Value.RetryIntervals);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.BackgroundServiceStarting();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        return ExecuteAsync(_cancellationTokenSource.Token);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.BackgroundServiceStopping();
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        QueueMessage<GameDto>? queueMessage;

        foreach (var interval in _retryState)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                queueMessage = await _gameQueueService.DequeueMessageAsync();
            }
            catch (NoItemException)
            {
                _logger.NoItemInTheQueue();
                await Task.Delay(interval * 1000, cancellationToken);
                continue;
            }

            Data.ReportService.Models.Game dbGame = queueMessage.Body.ToModel();

            try
            {
                await _gameRepository.CreateAsync(dbGame, cancellationToken);
            }
            catch (RepositoryException ex)
            {
                _logger.LogError(ex, "Error storing the game with the id {id} to the database", queueMessage.Id);
                continue;
            }

            await _gameQueueService.RemoveMessageAsync(queueMessage, cancellationToken); // Remove the message from the queue, since it was successfully processed.
            _retryState.Reset();
        }

        _logger.BackgroundServiceFinished();
    }
}

internal record class RetryState(RetryInterval[] RetryIntervals) : IEnumerable<int>
{
    private readonly IEnumerator<int> _enumerator = new RetryStateEnumerator(RetryIntervals);

    public void Reset() => _enumerator.Reset();

    public IEnumerator<int> GetEnumerator() => _enumerator;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private class RetryStateEnumerator : IEnumerator<int>
    {
        private readonly RetryInterval[] _retryIntervals;

        private int _currentItemIndex = 0;

        private int _useCount = -1;

        public RetryStateEnumerator(RetryInterval[] retryIntervals)
        {
            _retryIntervals = retryIntervals;
        }

        public RetryInterval CurrentItem => _retryIntervals[_currentItemIndex];

        public int Current => CurrentItem.Interval;

        object? IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            // If reuse == -1 the current interval should be used forever
            if (CurrentItem.Use == -1)
                return true;

            // The next "use" within the same interval
            _useCount++;

            // The current maximum reuse count is reached
            if (CurrentItem.Use == 0 || _useCount == CurrentItem.Use)
            {
                // All intervals were used -> end
                if (_currentItemIndex == _retryIntervals.Length - 1)
                {
                    Reset();
                    return false;
                }

                // Move to the next interval
                _currentItemIndex++;
                _useCount = 0;
                return true;
            }

            return true;
        }

        public void Reset()
        {
            _currentItemIndex = 0;
            _useCount = 0;
        }
    }
}
