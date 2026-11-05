using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Seasons;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public class CollegeRivalryTests
{
    [Fact]
    public void SeasonRolloverRecordsTheRivalrySet()
    {
        var next = SeasonProgression.Advance(State());

        var rivalry = Assert.Single(next.Rivalries);
        Assert.Equal("Oregon|Washington", rivalry.Id);
        Assert.Equal(3, rivalry.Games.Count);
    }

    [Fact]
    public void OlderSaveWithoutRivalriesLoadsWithAnEmptyCollection()
    {
        const string legacy = "{\"DynastyName\":\"Legacy\",\"UserTeamName\":\"Washington\",\"SeasonYear\":2026}";
        var restored = JsonSerializer.Deserialize<DynastyState>(legacy)!;

        Assert.Empty(restored.Rivalries);
    }

    [Fact]
    public void RivalriesSurviveSaveRoundTrip()
    {
        var state = SeasonProgression.Advance(State());
        var restored = JsonSerializer.Deserialize<DynastyState>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.Rivalries.Count, restored.Rivalries.Count);
        Assert.Equal(state.Rivalries[0].Id, restored.Rivalries[0].Id);
        Assert.Equal(state.Rivalries[0].Games.Count, restored.Rivalries[0].Games.Count);
    }

    static DynastyState State()
    {
        var team = new Team { Name = "Washington", Abbreviation = "WASH", ConferenceName = "ACC", Prestige = 70 };
        var rival = new Team { Name = "Oregon", Abbreviation = "ORE", ConferenceName = "PAC", Prestige = 72 };
        var schedule = new List<ScheduledGame>
        {
            Game("2025-1", "Washington", "Oregon", 2025, 1),
            Game("2025-9", "Oregon", "Washington", 2025, 9),
            Game("2026-3", "Washington", "Oregon", 2026, 3),
            Game("2026-5", "Washington", "Michigan", 2026, 5)
        };
        return new DynastyState
        {
            DynastyName = "Test Dynasty",
            UserTeamName = team.Name,
            SeasonYear = 2026,
            Week = 27,
            Phase = SeasonPhase.Offseason,
            LeagueTeams = [team, rival],
            Schedule = schedule.ToArray()
        };
    }

    static ScheduledGame Game(string gameId, string home, string away, int year, int week) => new()
    {
        GameId = gameId, SeasonYear = year, Week = week,
        HomeTeamName = home, AwayTeamName = away, HasPlayed = true,
        HomeScore = 21, AwayScore = 7
    };
}