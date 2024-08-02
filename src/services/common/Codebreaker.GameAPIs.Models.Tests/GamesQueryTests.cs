using Codebreaker.GameAPIs.Data;

namespace Codebreaker.GameAPIs.Models.Tests;

public class GamesQueryTests
{
    [Fact]
    public void CreatedGame_Should_JustReturnEndedAndRunningOnly()
    {
        string expected = "Ended:True,RunningOnly:False";
        GamesQuery query = new();
        string actual = query.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreatedGame_Should_ShowGameType()
    {
        string expected = "GameType:Game6x4,Ended:True,RunningOnly:False";
        GamesQuery query = new("Game6x4");
        string actual = query.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreatedGame_Should_ShowGameTypeAndDate()
    {
        string expected = "GameType:Game6x4,Date:02/05/2024,Ended:True,RunningOnly:False";
        GamesQuery query = new("Game6x4", Date: new DateOnly(2024, 2, 5));
        string actual = query.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreatedGame_Should_ShowPlayerName()
    {
        string expected = "PlayerName:Tom,Ended:True,RunningOnly:False";
        GamesQuery query = new(PlayerName: "Tom");
        string actual = query.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreatedGame_Should_ShowEndedFalse()
    {
        string expected = "Ended:False,RunningOnly:False";
        GamesQuery query = new(Ended: false);
        string actual = query.ToString();
        Assert.Equal(expected, actual);
    }
}
