using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Seasons;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public class CollegeHallOfFameTests
{
    [Fact]
    public void SeasonRolloverInductsACompletedCoachCareer()
    {
        var next = SeasonProgression.Advance(State());

        var entry = Assert.Single(next.CoachHallOfFame);
        Assert.Equal("Retired Coach", entry.CoachName);
        Assert.Equal(2027, entry.InductionYear);
        Assert.Equal(8, entry.HeadCoachSeasons);
        Assert.Equal(1, entry.ConferenceTitles);
        Assert.NotEmpty(entry.TeamCitations);
        Assert.Contains("Washington 2018-2025", entry.TeamCitations[0]);
    }

    [Fact]
    public void ACoachStillOnTheStaffIsNotInducted()
    {
        var state = State();
        var active = new StaffMember
        {
            StaffId = state.CoachSeasonHistory[0].StaffId,
            FullName = "Retired Coach",
            TeamName = "Washington",
            Role = StaffRole.HeadCoach
        };

        var next = SeasonProgression.Advance(state with { Staff = [active] });

        Assert.Empty(next.CoachHallOfFame);
    }

    [Fact]
    public void InductionIsRecordedOnceAndKeepsItsYear()
    {
        var first = SeasonProgression.Advance(State());
        var second = SeasonProgression.Advance(first with { Phase = SeasonPhase.Offseason, Week = 27 });

        var entry = Assert.Single(second.CoachHallOfFame);
        Assert.Equal(2027, entry.InductionYear);
    }

    [Fact]
    public void OlderSaveWithoutCoachInductionsLoadsWithAnEmptyCollection()
    {
        const string legacy = "{\"DynastyName\":\"Legacy\",\"UserTeamName\":\"Washington\",\"SeasonYear\":2026}";
        var restored = JsonSerializer.Deserialize<DynastyState>(legacy)!;

        Assert.Empty(restored.CoachHallOfFame);
    }

    [Fact]
    public void CoachInductionsSurviveSaveRoundTrip()
    {
        var state = SeasonProgression.Advance(State());
        var restored = JsonSerializer.Deserialize<DynastyState>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.CoachHallOfFame.Count, restored.CoachHallOfFame.Count);
        Assert.Equal(
            state.CoachHallOfFame[0].InductionYear,
            restored.CoachHallOfFame[0].InductionYear);
        Assert.Equal(
            state.CoachHallOfFame[0].TeamCitations,
            restored.CoachHallOfFame[0].TeamCitations);
    }

    static DynastyState State(int seasonYear = 2026)
    {
        var team = new Team
        {
            Name = "Washington",
            Abbreviation = "WASH",
            ConferenceName = "ACC",
            Prestige = 70
        };
        var coachId = Guid.NewGuid();
        var seasons = new List<CoachSeasonRecord>();
        for (var i = 0; i < 8; i++)
        {
            seasons.Add(new CoachSeasonRecord
            {
                StaffId = coachId,
                CoachName = "Retired Coach",
                SeasonYear = 2018 + i,
                TeamName = team.Name,
                Wins = 9,
                Losses = 3,
                Role = StaffRole.HeadCoach,
                ConferenceChampion = i < 1,
                PlayoffParticipant = i < 3,
                NationalChampion = i < 1
            });
        }

        return new DynastyState
        {
            DynastyName = "Test Dynasty",
            UserTeamName = team.Name,
            SeasonYear = seasonYear,
            Week = 27,
            Phase = SeasonPhase.Offseason,
            LeagueTeams = [team],
            Staff =
            [
                new StaffMember
                {
                    StaffId = Guid.NewGuid(),
                    FullName = "Current Coach",
                    TeamName = team.Name,
                    Role = StaffRole.HeadCoach
                }
            ],
            CoachSeasonHistory = seasons.ToArray()
        };
    }
}