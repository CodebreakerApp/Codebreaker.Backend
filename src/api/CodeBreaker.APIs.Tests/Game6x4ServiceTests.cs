using CodeBreaker.APIs.Data;
using CodeBreaker.APIs.Exceptions;
using CodeBreaker.APIs.Services;
using CodeBreaker.Shared;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace CodeBreaker.APIs.Tests;

public class Game6x4ServiceTests
{
    [Fact]
    public async Task ShouldThrowOnInvalidGuesses()
    {
        Mock<ILogger<GameAlgorithm>> logger = new();
        Mock<IGameAlgorithm> algorithm = new();

        Game aGame = new("4711", "6x4", "test", new string[] { "Blue" }, new string[] { "Blue" }, 4, 12, DateTime.Now);
        Mock<IGameCache> cache = new();
        cache.Setup(c => c.GetGame("4711")).Returns(() => aGame);
        Mock<ICodeBreakerContext> dataContext = new();
        
        Game6x4Service service = new(new Game6x4Definition(), algorithm.Object, cache.Object, dataContext.Object, new TestLogger<Game6x4Service>());

        await Assert.ThrowsAsync<GameMoveException>(async () =>
        {
            GameMove guess = new("4711", 1, new string[] { "Blue" });
            await service.SetMoveAsync(guess);
        });
    }
}
