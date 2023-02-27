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

        // expecting about this JSON
        //string expected = """
        //    {
        //        "gameTypes": [
        //        {
        //            "name": "8x5Game"
        //        },
        //        {
        //            "name": "6x4MiniGame"
        //        },
        //        {
        //            "name": "6x4Game"
        //        }
        //      ]
        //    }
        //    """;

        // JsonElement jsonExpected = JsonDocument.Parse(expected).RootElement;
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
}
