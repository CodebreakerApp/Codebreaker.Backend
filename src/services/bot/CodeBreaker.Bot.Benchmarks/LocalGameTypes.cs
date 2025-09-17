using System.Text.Json.Serialization;

namespace CodeBreaker.Bot.Benchmarks;

/// <summary>
/// Local copy of GameType enum to avoid external dependencies
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GameType>))]
public enum GameType
{
    Game6x4,
    Game6x4Mini,
    Game8x5,
    Game5x5x4
}