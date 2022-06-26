using CodeBreaker.APIs.Data;
using CodeBreaker.APIs.Extensions;
using CodeBreaker.Shared;

using Xunit;

namespace CodeBreaker.APIs.Tests;

public class CodeBreakerExtensionsTests
{
    [Fact]
    public void TestToGame()
    { 
        Game expected = new("79fa64d6-9ba7-4e56-90b6-8ed7aba3c6e1", GameTypes.Game6x4, "test", new[] {"green", "green", "yellow", "green"} );
        CodeBreakerGame dbGame = new("79fa64d6-9ba7-4e56-90b6-8ed7aba3c6e1", GameTypes.Game6x4, "green..green..yellow..green", "test", DateTime.Now);
        Game actual = CodeBreakerExtensions.ToGame(dbGame);
        Assert.Equal(expected, actual);
    }
}
