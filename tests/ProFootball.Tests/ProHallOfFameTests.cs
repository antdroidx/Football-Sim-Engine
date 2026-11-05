using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class ProHallOfFameTests
{
    [Fact]
    public void HallOfFameIncludesQualifiedRetiredPlayer()
        => Assert.NotEmpty(ProHallOfFameService.Build(State(true)));

    [Fact]
    public void HallOfFameExcludesActivePlayers()
        => Assert.Empty(ProHallOfFameService.Build(State(false)));

    static ProFranchise State(bool retired)
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var league = new ProLeague(1, "Test League", 53, 16, [team]);
        var playerId = Guid.NewGuid();
        var football = new DynastyPlayer
        {
            PlayerId = playerId,
            FullName = "Test QB",
            TeamName = team.Name,
            Position = Position.QB,
            OverallRating = 80
        };
        var player = new ProPlayer(football, 33, 11, false) with { Retired = retired };
        var game = new ScheduledGame
        {
            GameId = "game-1",
            SeasonYear = 2026,
            Week = 1,
            HomeTeamName = team.Name,
            AwayTeamName = "Away",
            HasPlayed = true,
            HomeScore = 27,
            AwayScore = 20,
            PlayerStats =
            [
                new PlayerSeasonStatLine
                {
                    PlayerId = playerId,
                    PlayerName = player.Football.FullName,
                    TeamName = team.Name,
                    Position = Position.QB,
                    Games = 1,
                    PassAttempts = 30,
                    PassCompletions = 20,
                    PassYards = 300,
                    PassingTouchdowns = 3,
                    SeasonYear = 2026
                }
            ]
        };
        return new ProFranchise
        {
            League = league,
            UserTeam = team.Name,
            Year = 2026,
            Week = 18,
            Phase = ProPhase.Offseason,
            Players = [player],
            Games = [new ProGame(game, ProPhase.RegularSeason)],
            Honors =
            [
                new ProSeasonHonors(2026, [new ProAward("Most Valuable Player", playerId, player.Football.FullName, team.Name, 1)], [])
            ]
        };
    }
}
