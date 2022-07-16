using CodeBreaker.APIs.Data;
using CodeBreaker.APIs.Extensions;
using CodeBreaker.APIs.Services.Obsolete;
using CodeBreaker.Shared;

using Xunit;

using static CodeBreaker.Shared.CodeBreakerColors;

namespace CodeBreaker.APIs.Tests;

public class CodeBreakerExtensionsTests
{
    [Fact]
    public void TestToGame()
    {
        Game6x4Definition definition = new();
        Game_Old expected = new("79fa64d6-9ba7-4e56-90b6-8ed7aba3c6e1", GameTypes.Game6x4, "test", new[] { Green, Green, Yellow, Green},  definition.Colors, definition.Holes, definition.MaxMoves, DateTime.Now);
        CodeBreakerGame dbGame = new("79fa64d6-9ba7-4e56-90b6-8ed7aba3c6e1", GameTypes.Game6x4, "Green..Green..Yellow..Green", "test", expected.Holes, expected.ColorList.ToArray(), definition.MaxMoves, expected.StartTime);
        Game_Old actual = CodeBreakerExtensions.ToGame(dbGame);
        Assert.Equal(expected, actual);
    }
}
