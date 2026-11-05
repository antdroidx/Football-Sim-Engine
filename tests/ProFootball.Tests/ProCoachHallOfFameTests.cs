using System.Text.Json;
using DynastyManager.Core.Models;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class ProCoachHallOfFameTests
{
    [Fact]
    public void ACompletedProCoachingCareerIsInducted()
    {
        var next = ProCoachHallOfFameService.Refresh(State(unemployed: true));

        var entry = Assert.Single(next.CoachHallOfFame);
        Assert.Equal("Test Coach", entry.CoachName);
        Assert.Equal(2026, entry.InductionYear);
        Assert.Equal(8, entry.HeadCoachSeasons);
        Assert.Equal(1, entry.NationalTitles);
        Assert.NotEmpty(entry.TeamCitations);
    }

    [Fact]
    public void ACoachStillWorkingIsNotInducted()
    {
        var next = ProCoachHallOfFameService.Refresh(State(unemployed: false));

        Assert.Empty(next.CoachHallOfFame);
    }

    [Fact]
    public void InductionIsRecordedOnceAndKeepsItsYear()
    {
        var first = ProCoachHallOfFameService.Refresh(State(unemployed: true));
        var second = ProCoachHallOfFameService.Refresh(first with { Year = 2027 });

        var entry = Assert.Single(second.CoachHallOfFame);
        Assert.Equal(2026, entry.InductionYear);
    }

    [Fact]
    public void OlderSaveWithoutCoachInductionsLoadsWithAnEmptyCollection()
    {
        var legacy = JsonSerializer.Serialize(State(unemployed: true))
            .Replace("\"CoachHallOfFame\":[],", "");
        var restored = JsonSerializer.Deserialize<ProFranchise>(legacy)!;

        Assert.Empty(restored.CoachHallOfFame);
    }

    [Fact]
    public void CoachInductionsSurviveSaveRoundTrip()
    {
        var state = ProCoachHallOfFameService.Refresh(State(unemployed: true));
        var restored = JsonSerializer.Deserialize<ProFranchise>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.CoachHallOfFame.Count, restored.CoachHallOfFame.Count);
        Assert.Equal(state.CoachHallOfFame[0].InductionYear, restored.CoachHallOfFame[0].InductionYear);
    }

    static ProFranchise State(bool unemployed)
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var league = new ProLeague(1, "Test League", 53, 16, [team]);
        var coaching = new List<ProCareerSeason>();
        for (var i = 0; i < 8; i++)
            coaching.Add(new ProCareerSeason(2018 + i, team.Name, StaffRole.HeadCoach, 11, 5, 80, 90, false));

        var archives = new List<SeasonArchive>();
        for (var i = 0; i < 4; i++)
        {
            var year = 2018 + i;
            archives.Add(new SeasonArchive(year, year == 2020 ? team.Name : "Other Team",
            [
                new ProGame(new ScheduledGame
                {
                    GameId = $"playoff-{year}",
                    SeasonYear = year,
                    Week = 1,
                    HomeTeamName = team.Name,
                    AwayTeamName = "Other Team"
                }, ProPhase.Playoffs)
            ]));
        }

        return new ProFranchise
        {
            Name = "Test franchise",
            League = league,
            UserTeam = team.Name,
            Year = 2026,
            Week = 1,
            Career = new ProUserCareer("Test Coach", StaffRole.HeadCoach, false) { Unemployed = unemployed },
            CoachingHistory = coaching,
            History = archives
        };
    }
}