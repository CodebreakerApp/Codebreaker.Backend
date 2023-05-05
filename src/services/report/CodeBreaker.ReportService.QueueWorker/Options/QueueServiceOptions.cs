namespace CodeBreaker.ReportService.QueueWorker.Options;

public class QueueServiceOptions
{
    /// <summary>
    /// Gets or sets the retry intervals.
    /// </summary>
    /// <value>
    /// The retry intervals define the number of seconds the service will wait between the dequeue-attempts.                       <br />
    /// (Interval, Use) ... <b>Interval</b> defines the number of seconds; <b>Use</b> defines the number of times a will be used.  <br />
    /// e.g. Use=10 means, that the related interval (<b>Interval</b>) will be used 10 times.                                      <br />
    /// Use=-1 ... The first interval will be used forever.                                                                        <br />
    /// Use= 0 ... The interval will not be used.                                                                                  <br />
    /// Use= 1 ... Every interval will be used once.                                                                               <br />
    /// ...
    /// </value>
    /// <remarks>
    /// An empty array means that no retry will be performed. <br />
    /// After all retry intervals were handled, the service will terminate.
    /// </remarks>
    public RetryInterval[] RetryIntervals { get; set; } = Array.Empty<RetryInterval>();
}

public readonly record struct RetryInterval(int Interval, int Use)
{
    public int I
    {
        get => Interval;
        init => Interval = value;
    }

    public int U
    {
        get => Use;
        init => Use = value;
    }
}
