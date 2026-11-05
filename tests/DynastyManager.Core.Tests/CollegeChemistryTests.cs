using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Seasons;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public class CollegeChemistryTests
{
    [Fact]
    public void SeasonRolloverWritesCultureAndLockerRoomRecords()
    {
        var next = SeasonProgression.Advance(State(SeasonPhase.Offseason));

        Assert.Equal(SeasonPhase.Preseason, next.Phase);
        Assert.Equal(2027, next.SeasonYear);

        var record = CollegeChemistryService.Record(next, "Washington", 2027);
        Assert.NotNull(record);
        Assert.InRange(record!.Cohesion, TeamChemistrySeasonService.Minimum, TeamChemistrySeasonService.Maximum);
        Assert.InRange(record.SchemeFamiliarity, TeamChemistrySeasonService.Minimum, TeamChemistrySeasonService.Maximum);
        Assert.NotEmpty(record.Notes);

        Assert.NotEmpty(next.LockerRoomEvents);
        Assert.All(next.LockerRoomEvents, item => Assert.StartsWith("locker/", item.Id));
        Assert.Contains(next.LockerRoomEvents, item => item.TeamName == "Washington");
    }

    [Fact]
    public void CultureContinuityCarriesIntoTheNextSeason()
    {
        var first = SeasonProgression.Advance(State(SeasonPhase.Offseason));
        var second = SeasonProgression.Advance(first with { Phase = SeasonPhase.Offseason, Week = 27 });

        Assert.Equal(2028, second.SeasonYear);
        Assert.Equal(2, second.TeamChemistry.Count(record => record.TeamName == "Washington"));

        var latest = CollegeChemistryService.Record(second, "Washington", 2028);
        Assert.NotNull(latest);
        Assert.Contains(latest!.Notes, note => note.Contains("Head coach continuity"));
        Assert.Contains(latest.Notes, note => note.Contains("same game plan"));
    }

    [Fact]
    public void RosterTurnoverAndACoachChangeLowerCohesion()
    {
        var first = SeasonProgression.Advance(State(SeasonPhase.Offseason));
        var prior = CollegeChemistryService.Record(first, "Washington", 2027)!;

        var changed = first with
        {
            Phase = SeasonPhase.Offseason,
            Week = 27,
            Staff = first.Staff.Select(staff => staff with
            {
                StaffId = Guid.NewGuid(),
                FullName = "Replacement Coach"
            }).ToArray(),
            PlayerDepartureHistory =
            [
                .. first.PlayerDepartureHistory,
                new PlayerDepartureRecord
                {
                    SeasonYear = 2027,
                    PlayerId = Guid.NewGuid(),
                    FullName = "Departed Player",
                    TeamName = "Washington",
                    Position = Position.RB
                }
            ]
        };

        var next = SeasonProgression.Advance(changed);
        var record = CollegeChemistryService.Record(next, "Washington", 2028)!;

        Assert.True(record.Cohesion < prior.Cohesion);
        Assert.Contains(record.Notes, note => note.Contains("new head coach"));
        Assert.Contains(record.Notes, note => note.Contains("Roster turnover"));
    }

    [Fact]
    public void OlderSaveWithoutCultureRecordsLoadsWithEmptyCollections()
    {
        const string legacy = "{\"DynastyName\":\"Legacy\",\"UserTeamName\":\"Washington\",\"SeasonYear\":2026,\"Week\":4,\"Phase\":1}";
        var restored = JsonSerializer.Deserialize<DynastyState>(legacy)!;

        Assert.Empty(restored.TeamChemistry);
        Assert.Empty(restored.LockerRoomEvents);
        Assert.Equal("Legacy", restored.DynastyName);
    }

    [Fact]
    public void CultureRecordsSurviveSaveRoundTrip()
    {
        var state = SeasonProgression.Advance(State(SeasonPhase.Offseason));
        var restored = JsonSerializer.Deserialize<DynastyState>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.TeamChemistry.Count, restored.TeamChemistry.Count);
        Assert.Equal(state.LockerRoomEvents.Count, restored.LockerRoomEvents.Count);
        Assert.Equal(
            CollegeChemistryService.Record(state, "Washington", 2027)!.Cohesion,
            CollegeChemistryService.Record(restored, "Washington", 2027)!.Cohesion);
    }

    static DynastyState State(SeasonPhase phase)
    {
        var team = new Team
        {
            Name = "Washington",
            Abbreviation = "WASH",
            ConferenceName = "ACC",
            Prestige = 70
        };
        var coach = new StaffMember
        {
            StaffId = Guid.NewGuid(),
            FullName = "Test Coach",
            TeamName = team.Name,
            Role = StaffRole.HeadCoach,
            Coaching = new CoachingAttributes
            {
                Leadership = 70,
                OffensePlaycalling = 70,
                DefenseScheme = 70
            }
        };
        var roster = new[]
        {
            Player(team.Name, Position.QB, 80, 1),
            Player(team.Name, Position.RB, 76, 2),
            Player(team.Name, Position.WR, 74, 3),
            Player(team.Name, Position.OL, 72, 4),
            Player(team.Name, Position.DE, 71, 5),
            Player(team.Name, Position.CB, 70, 6)
        };
        return new DynastyState
        {
            DynastyName = "Test Dynasty",
            UserTeamName = team.Name,
            SeasonYear = 2026,
            Week = 27,
            Phase = phase,
            LeagueTeams = [team],
            Staff = [coach],
            ActiveRoster = roster,
            DisciplineHistory =
            [
                new DisciplineIncident
                {
                    SeasonYear = 2026,
                    Week = 9,
                    TeamName = team.Name,
                    EventId = "incident-1",
                    Title = "Practice incident",
                    Description = "Arrived late to a team meeting.",
                    Severity = 2
                }
            ]
        };
    }

    static DynastyPlayer Player(string teamName, Position position, int overall, int depth) => new()
    {
        PlayerId = Guid.NewGuid(),
        FullName = $"{position} {depth}",
        TeamName = teamName,
        Position = position,
        OverallRating = overall,
        PotentialRating = overall,
        DepthChartOrder = depth,
        AwarenessRating = overall,
        SpeedRating = overall,
        StrengthRating = overall,
        AgilityRating = overall,
        InjuryRating = 80,
        Skills = new PlayerSkillRatings
        {
            PassingPower = overall,
            PassingAccuracy = overall,
            Catching = overall,
            BreakTackle = overall,
            Tackling = overall,
            RunBlocking = overall,
            PassBlocking = overall,
            BallCarrying = overall,
            Acceleration = overall,
            Leadership = overall,
            Discipline = 80,
            Jumping = overall,
            KickPower = overall,
            KickAccuracy = overall
        }
    };
}