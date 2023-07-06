using System.Text.Json;
using Microsoft.Playwright;

namespace CodeBreaker.ReportService.IntegrationTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class IntegrationTests : PlaywrightTest
{
    private readonly static string s_webapiBaseUrl = "https://localhost:7192";//"https://codebreaker-report-webapi.purplebush-9a246700.westeurope.azurecontainerapps.io";

    private IAPIRequestContext? _request;

    [SetUp]
    public async Task SetupApiTesting()
    {
        _request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = s_webapiBaseUrl,
            IgnoreHTTPSErrors = true,
        });
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        if (_request != null)
            await _request.DisposeAsync();
    }

    [Test]
    public async Task ShouldGetGames()
    {
        if (_request is null)
        {
            Assert.Fail();
            return;
        }

        var response = await _request.GetAsync("/games");
        Assert.True(response.Ok, "The response code returned by the api does not indicate a successful operation.");

        var json = await response.JsonAsync();

        int gameLength = 0;
        Assert.DoesNotThrow(() => gameLength = json.Value.GetArrayLength());

        if (gameLength == 0)
            Assert.Warn("The returned array length is 0. This is no error, but the test cannot continue.");

        bool everyMoveEmpty = true;
        foreach (var game in json.Value.EnumerateArray())
        {
            if (game.GetProperty("id").GetGuid() == default)
                Assert.Fail("The id of the game must not be a default GUID.");

            if (game.GetProperty("type").GetString() == default)
                Assert.Fail("The type of the game must not be a default GUID.");

            if (game.GetProperty("start").GetDateTime() == default)
                Assert.Fail("The start of the game must not be a default Datetime.");

            int moveLength = 0;
            Assert.DoesNotThrow(() => moveLength = game.GetProperty("moves").GetArrayLength(), "The game has to contain moves.");

            if (moveLength != 0)
                everyMoveEmpty = false;
        }

        if (gameLength != 0 && everyMoveEmpty)
            Assert.Warn("No game returned by the API did contain any moves.");
    }

    [Test]
    public async Task ShouldGetOdataGames()
    {

        if (_request is null)
        {
            Assert.Fail();
            return;
        }

        var response = await _request.GetAsync("/games?$select=id");
        Assert.True(response.Ok, "The response code returned by the api does not indicate a successful operation.");

        var json = await response.JsonAsync();

        int gameLength = 0;
        Assert.DoesNotThrow(() => gameLength = json.Value.GetArrayLength());

        if (gameLength == 0)
            Assert.Warn("The returned array length is 0. This is no error, but the test cannot continue.");

        foreach (var game in json.Value.EnumerateArray())
        {
            JsonElement idProperty;

            if (!game.TryGetProperty("id", out idProperty) && !game.TryGetProperty("Id", out idProperty))
                Assert.Fail("The result item does not contain an id.");

            if (idProperty.GetGuid() == default)
                Assert.Fail("The id of the game must not be a default GUID.");

            if (game.TryGetProperty("type", out _) || game.TryGetProperty("Type", out _))
                Assert.Fail("The response contains data, not requested by the OData query");
        }
    }
}
