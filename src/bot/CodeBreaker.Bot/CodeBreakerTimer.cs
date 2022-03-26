using System.Collections.Concurrent;

namespace CodeBreaker.Bot;

public class CodeBreakerTimer
{
    private readonly CodeBreakerGameRunner _gameRunner;
    private readonly ILogger _logger;
    private static readonly ConcurrentDictionary<Guid, CodeBreakerTimer> s_bots = new ConcurrentDictionary<Guid, CodeBreakerTimer>();
    private PeriodicTimer? _timer;
    private int _loop = 0;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public CodeBreakerTimer(CodeBreakerGameRunner runner, ILogger<CodeBreakerTimer> logger)
    {
        _gameRunner = runner;
        _logger = logger;
    }

    public string Start(int delaySeconds, int loops)
    { 
        _logger.LogInformation("Starting MMGameRunner");
        Guid id = Guid.NewGuid();
        s_bots.TryAdd(id, this);

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(delaySeconds));
        var _ = Task.Factory.StartNew(async () =>
        {
            do
            {
                _logger.LogInformation("bot - waiting for tick in loop {loop}", _loop);
                bool letsgo = await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token);
                if (letsgo)
                {
                    _logger.LogInformation("bot - tick activated in loop {loop}", _loop);
                    await _gameRunner.StartGameAsync();
                    await _gameRunner.SetMovesAsync();
                    _loop++;
                }
            } while (_loop < loops);

        }, TaskCreationOptions.LongRunning);
        return id.ToString();
    }

    public void Stop()
    {
        _timer?.Dispose();
    }

    public string Status()
    {
        return $"running with loop {_loop}";
    }

    public static string Stop(string id)
    {
        if (TryGetId(id, out Guid guid))
        {
            if (s_bots.TryGetValue(guid, out CodeBreakerTimer? timer))
            {
                timer?.Stop();
                s_bots.TryRemove(guid, out CodeBreakerTimer? _);
            }
            return "thanks";
        }
        else
        {
            return "invalid id";
        }
    }

    private static bool TryGetId(string id, out Guid guid)
    {
        try
        {
            guid = Guid.Parse(id);
        }
        catch (FormatException)
        {
            guid = Guid.Empty;
            return false;
        }
        return true;
    }

    public static string Status(string id)
    {
        if (TryGetId(id, out Guid guid))
        {
            if (s_bots.TryGetValue(guid, out CodeBreakerTimer? timer))
            {
                string status = timer?.Status() ?? "id found, but unknown status";
                return status;
            }
            return "not running";
        }
        else
        {
            return "invalid id";
        }
    }
}
