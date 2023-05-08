using Codebreaker.GameAPIs.Models;
using Codebreaker.GameAPIs.Models.Factories;

using static Azure.Core.HttpHeader;

namespace Codebreaker.GameAPIs.Data.Cosmos.Tests;

public class CodebreakerCosmosRepositoryTest : IClassFixture<TestCosmosFixture>
{
    private TestCosmosFixture Fixture { get; }
    public CodebreakerCosmosRepositoryTest(TestCosmosFixture fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public void Test1()
    {
        using var context = Fixture.CreateContext();

        var game = GamesFactory.CreateGame(GameType.Game6x4, "testuser");
        context.Add(game);
        // context.Games.Add(game);
        
        context.SaveChanges();
    }
}
