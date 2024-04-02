namespace Codebreaker.GameAPIs.Models;

public record class GameSummary(
    Guid Id,
    string GameType,
    string PlayerName,
    bool IsCompleted,
    bool IsVictory,
    int NumberMoves,
    DateTime StartTime,
    TimeSpan Duration)
{
    public override string ToString() => $"{Id}:{GameType}, victory: {IsVictory}, duration: {Duration}";
}
