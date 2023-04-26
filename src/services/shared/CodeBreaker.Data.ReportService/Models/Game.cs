using CodeBreaker.Data.Common.Models;

namespace CodeBreaker.Data.ReportService.Models;

public class Game : IIdentifyable<Guid>
{
    public Guid Id { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }
}
