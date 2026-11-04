using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using System.Text.Json;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class RivalryServiceTests
{
    [Fact]
    public void DeriveUsesRepeatedPairingsAndStableSeriesIdentity()
    {
        var games = new[]
        {
            Game("a-b-once", "A", "B", 2026, 2),
            Game("a-b-current", "B", "A", 2027, 3),
            Game("a-c", "A", "C", 2027, 1)
        };

        var rivalries = RivalryService.Derive(games);

        var rivalry = Assert.Single(rivalries);
        Assert.Equal("A|B", rivalry.Id);
        Assert.Equal("A", rivalry.TeamAName);
        Assert.Equal("B", rivalry.TeamBName);
        Assert.Equal(2, rivalry.Games.Count);
        Assert.True(rivalry.Involves("B"));
        Assert.True(rivalry.Involves("B", "A"));
        Assert.False(rivalry.Involves("C"));
        Assert.True(RivalryService.IsRivalry(games, "B", "A"));
        Assert.Equal("Rivalry", RivalryService.Label(games, "B", "A"));
        Assert.Empty(RivalryService.Label(games, "A", "C"));
    }

    [Fact]
    public void RivalryRecordsSurviveSerialization()
    {
        var games = new[]
        {
            Game("a-b-once", "A", "B", 2026, 2),
            Game("a-b-current", "B", "A", 2027, 3)
        };

        var before = RivalryService.Derive(games);
        var json = JsonSerializer.Serialize(before);
        var after = JsonSerializer.Deserialize<RivalryRecord[]>(json);

        Assert.NotNull(after);
        Assert.Equal(json, JsonSerializer.Serialize(after));
    }

    private static ScheduledGame Game(string gameId, string home, string away, int year, int week) => new()
    {
        GameId = gameId, SeasonYear = year, Week = week,
        HomeTeamName = home, AwayTeamName = away, HasPlayed = true,
        HomeScore = 21, AwayScore = 7, SimulationSeed = 1234
    };
}

