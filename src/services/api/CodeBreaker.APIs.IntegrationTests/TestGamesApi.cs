using System.Text.Json;

using Microsoft.Playwright;

namespace Codebreaker.Apis.IntegrationTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class TestGamesApi : PlaywrightTest
{
    private readonly string _baseUrl = "https://codebreakerapi-2.purplebush-9a246700.westeurope.azurecontainerapps.io/";
    private IAPIRequestContext? _request = default;

    [SetUp]
    public async Task SetupApiTesting()
    {
        await CreateAPIRequestContext();
    }

    private async Task CreateAPIRequestContext()
    {
        // setup authentication when needed
        _request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = _baseUrl
        });
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        if (_request != null)
        {
            await _request.DisposeAsync();
        }
    }

    [Test]
    public async Task ShouldReturnGameTypes()
    {
        if (_request is null)
        {
            Assert.Fail();
            return;
        }

        var response = await _request.GetAsync($"{_baseUrl}/gametypes");
        Assert.That(response.Ok, Is.True);

        var json = await response.JsonAsync();
        Assert.DoesNotThrow(() =>
        {
            json.Value.GetProperty("gameTypes");
        });

        bool match = false;
        foreach (JsonElement gameType in json.Value.GetProperty("gameTypes").EnumerateArray())
        {
            string gameTypeName = gameType.GetProperty("name").ToString();
            if (gameTypeName == "6x4Game") match = true;  // at least this game type should be available
        }
        Assert.That(match, Is.True);
    }

    [Test]
    public async Task ShouldCreateA6x4GameAndReturnMoveResult()
    {
        if (_request is null)
        {
            Assert.Fail();
            return;
        }

        string username = "testuser";

        Dictionary<string, string> request = new()
        {
            ["username"] = username,
            ["gameType"] = "6x4Game"
        };

        var response = await _request.PostAsync($"{_baseUrl}/games", new()
        {
            DataObject = request
        });
        Assert.That(response.Ok, Is.True);

        var json = await response.JsonAsync();
        Assert.DoesNotThrow(() =>
        {
            json.Value.GetProperty("game");
            json.Value.GetProperty("game").GetProperty("type").GetProperty("name");
        });

        JsonElement jsonGame = json.Value.GetProperty("game");
        JsonElement jsonType = jsonGame.GetProperty("type");

        // is the game type the same as requested?
        Assert.That(jsonType.GetProperty("name").ToString(), Is.EqualTo("6x4Game"));

        Guid gameId = jsonGame.GetProperty("gameId").GetGuid();

        int holes = int.Parse(jsonType.GetProperty("holes").ToString());

        int maxMoves = int.Parse(jsonType.GetProperty("maxMoves").ToString());

        int length = jsonGame.GetProperty("moves").GetArrayLength();

        Assert.Multiple(() =>
        {
            // holes should be 4 with this game type
            Assert.That(holes, Is.EqualTo(4));

            // max moves should be 12 with this game type
            Assert.That(maxMoves, Is.EqualTo(12));

            // username returned should contain the username requested
            Assert.That(jsonGame.GetProperty("username").ToString(), Is.EqualTo(username));

            // moves should be an empty array
            Assert.That(length, Is.EqualTo(0));
        });

        Dictionary<string, object> moveRequest = new()
        {
            ["guessPegs"] = new string[] { "Red", "Blue", "Red", "Blue" }
        };

        response = await _request.PostAsync($"{_baseUrl}/games/{gameId}/moves", new()
        {
            DataObject = moveRequest
        });

        Assert.That(response.Ok, Is.True);

        json = await response.JsonAsync();
        JsonElement keyPegs = json.Value.GetProperty("keyPegs");
        int blackPegs = keyPegs.GetProperty("black").GetInt32();
        int whitePegs = keyPegs.GetProperty("white").GetInt32();

        Assert.Multiple(() =>
        {
            Assert.That(blackPegs, Is.LessThan(5));
            Assert.That(whitePegs, Is.LessThan(5));
        });

       // Dictionary<string, bool> deleteRequest = new()
       // {
       //     ["cancel"] = false
       // };
       // response = await _request.DeleteAsync($"{_baseUrl}/games/{gameId}", new()
       // {
       //     DataObject = deleteRequest
       // });

       // Assert.That(response.Ok, Is.True);
    }
}
