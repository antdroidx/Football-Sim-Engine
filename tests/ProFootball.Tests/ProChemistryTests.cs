using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class ProChemistryTests
{
    [Fact]
    public void RefreshWritesCultureAndLockerRoomRecords()
    {
        var state = ProChemistryService.Refresh(Season());

        var record = ProChemistryService.Record(state, "Seattle Seahawks", 2027);
        Assert.NotNull(record);
        Assert.InRange(record!.Cohesion, TeamChemistrySeasonService.Minimum, TeamChemistrySeasonService.Maximum);
        Assert.NotEmpty(record.Notes);

        Assert.NotEmpty(state.TeamMotivation);
        Assert.NotEmpty(state.LockerRoomEvents);
        Assert.All(state.LockerRoomEvents, item => Assert.StartsWith("locker/", item.Id));
        Assert.Contains(state.LockerRoomEvents, item => item.TeamName == "Seattle Seahawks");
    }

    [Fact]
    public void ContinuityCarriesIntoTheNextSeason()
    {
        var first = ProChemistryService.Refresh(Season());
        var second = ProChemistryService.Refresh(first with { Year = first.Year + 1 });

        Assert.Equal(2, second.TeamChemistry.Count(record => record.TeamName == "Seattle Seahawks"));
        var latest = ProChemistryService.Record(second, "Seattle Seahawks", second.Year)!;
        Assert.Contains(latest.Notes, note => note.Contains("Head coach continuity"));
        Assert.Contains(latest.Notes, note => note.Contains("same game plan"));
    }

    [Fact]
    public void ACoachChangeLowersCohesionAgainstAnUnchangedStaffControl()
    {
        var first = ProChemistryService.Refresh(Season());

        var control = ProChemistryService.Refresh(first with { Year = first.Year + 1 });
        var controlRecord = ProChemistryService.Record(control, "Seattle Seahawks", first.Year + 1)!;

        var changed = first with
        {
            Year = first.Year + 1,
            Coaches = first.Coaches.Select(coach => coach with { Name = "Replacement Coach" }).ToList()
        };
        var next = ProChemistryService.Refresh(changed);
        var record = ProChemistryService.Record(next, "Seattle Seahawks", changed.Year)!;

        Assert.True(record.Cohesion < controlRecord.Cohesion);
        Assert.Contains(record.Notes, note => note.Contains("new head coach"));
        Assert.Contains(next.LockerRoomEvents, item => item.Kind == LockerRoomEventKind.CoachingChange);
    }

    [Fact]
    public void OlderSaveWithoutCultureRecordsLoadsWithEmptyCollections()
    {
        // A save written before these collections existed simply has no keys for them.
        var legacy = System.Text.RegularExpressions.Regex.Replace(
            JsonSerializer.Serialize(Season()),
            "\"(TeamChemistry|LockerRoomEvents)\":\\[\\],",
            "");
        var restored = JsonSerializer.Deserialize<ProFranchise>(legacy)!;

        Assert.Empty(restored.TeamChemistry);
        Assert.Empty(restored.TeamMotivation);
        Assert.Empty(restored.LockerRoomEvents);
        Assert.Equal("Test franchise", restored.Name);
    }

    [Fact]
    public void CultureRecordsSurviveSaveRoundTrip()
    {
        var state = ProChemistryService.Refresh(Season());
        var restored = JsonSerializer.Deserialize<ProFranchise>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.TeamChemistry.Count, restored.TeamChemistry.Count);
        Assert.Equal(state.TeamMotivation.Count, restored.TeamMotivation.Count);
        Assert.Equal(state.LockerRoomEvents.Count, restored.LockerRoomEvents.Count);
        Assert.Equal(
            ProChemistryService.Record(state, "Seattle Seahawks", 2027)!.Cohesion,
            ProChemistryService.Record(restored, "Seattle Seahawks", 2027)!.Cohesion);
    }

    static ProFranchise Season()
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var league = new ProLeague(1, "Test League", 53, 16, [team]);
        var players = new[]
        {
            Player(team.Name, Position.QB, 82, 1),
            Player(team.Name, Position.RB, 78, 2),
            Player(team.Name, Position.WR, 76, 3),
            Player(team.Name, Position.OL, 74, 4),
            Player(team.Name, Position.DE, 73, 5),
            Player(team.Name, Position.CB, 72, 6)
        };
        return new ProFranchise
        {
            Name = "Test franchise",
            League = league,
            UserTeam = team.Name,
            Year = 2027,
            Week = 1,
            Phase = ProPhase.Preseason,
            Players = [.. players],
            Coaches =
            [
                new ProCoach(team.Id, StaffRole.HeadCoach, "Test Coach", "Head Coach", "Generated")
            ]
        };
    }

    static ProPlayer Player(string teamName, Position position, int overall, int depth)
    {
        var football = new DynastyPlayer
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
        return new ProPlayer(football, 28, 5, false) { Morale = 65 };
    }
}