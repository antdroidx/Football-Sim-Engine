using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class ProRivalryTests
{
    [Fact]
    public void RivalriesSurviveSaveRoundTrip()
    {
        var state = Franchise(rivalries: true);
        var restored = JsonSerializer.Deserialize<ProFranchise>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.Rivalries.Count, restored.Rivalries.Count);
        Assert.Equal(state.Rivalries[0].Id, restored.Rivalries[0].Id);
        Assert.Equal(state.Rivalries[0].Games.Count, restored.Rivalries[0].Games.Count);
    }

    [Fact]
    public void OlderSaveWithoutRivalriesLoadsWithAnEmptyCollection()
    {
        var legacy = JsonSerializer.Serialize(Franchise())
            .Replace("\"Rivalries\":[],", "");
        var restored = JsonSerializer.Deserialize<ProFranchise>(legacy)!;

        Assert.Empty(restored.Rivalries);
    }

    [Fact]
    public void BothGamesLabelTheSameMatchupTheSameWay()
    {
        var rivalries = RivalryRecordService.Derive(Games());

        Assert.Equal("Rivalry", RivalryRecordService.Label(rivalries, "Seattle Seahawks", "San Francisco 49ers"));
        Assert.Equal("Rivalry", RivalryRecordService.Label(rivalries, "San Francisco 49ers", "Seattle Seahawks"));
        Assert.Equal(string.Empty, RivalryRecordService.Label(rivalries, "Seattle Seahawks", "New York Jets"));
    }

    static ProFranchise Franchise() => Franchise(rivalries: false);

    static ProFranchise Franchise(bool rivalries)
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var league = new ProLeague(1, "Test League", 53, 16, [team]);
        var games = Games().Select(game => new ProGame(game, ProPhase.RegularSeason)).ToArray();
        return new ProFranchise
        {
            Name = "Test franchise",
            League = league,
            UserTeam = team.Name,
            Year = 2026,
            Week = 1,
            Games = [.. games],
            Rivalries = rivalries ? RivalryRecordService.Derive(Games()).ToList() : []
        };
    }

    static ScheduledGame[] Games() =>
    [
        Game("2025-1", "Seattle Seahawks", "San Francisco 49ers", 2025, 1, 24, 17),
        Game("2025-9", "San Francisco 49ers", "Seattle Seahawks", 2025, 9, 17, 24),
        Game("2026-3", "Seattle Seahawks", "New York Jets", 2026, 3, 20, 10)
    ];

    static ScheduledGame Game(string gameId, string home, string away, int year, int week, int homeScore, int awayScore) => new()
    {
        GameId = gameId, SeasonYear = year, Week = week,
        HomeTeamName = home, AwayTeamName = away, HasPlayed = true,
        HomeScore = homeScore, AwayScore = awayScore
    };
}