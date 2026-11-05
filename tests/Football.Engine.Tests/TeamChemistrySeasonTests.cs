using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class TeamChemistrySeasonTests
{
    static readonly Guid CaptainId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void FirstRecordedSeasonUsesTheEvaluatedSnapshot()
    {
        var record = TeamChemistrySeasonService.Advance(null, Inputs());

        Assert.Equal(60, record.Cohesion);
        Assert.Equal(58, record.SchemeFamiliarity);
        Assert.Equal(CohesionMomentum.Stable, record.Momentum);
        Assert.Contains(record.Notes, note => note.Contains("First recorded season"));
    }

    [Fact]
    public void TurnoverAndCoachingChangeLowerCohesion()
    {
        var prior = Prior();
        var record = TeamChemistrySeasonService.Advance(prior, Inputs() with
        {
            RosterTurnoverPercent = 40,
            HeadCoachKey = "coach-2",
            DisciplineSignal = -40
        });

        Assert.True(record.Cohesion < prior.Cohesion);
        Assert.Equal(CohesionMomentum.Falling, record.Momentum);
        Assert.Contains(record.Notes, note => note.Contains("Roster turnover of 40%"));
        Assert.Contains(record.Notes, note => note.Contains("new head coach"));
        Assert.Contains(record.Notes, note => note.Contains("Discipline"));
    }

    [Fact]
    public void ContinuityRaisesCohesionAndSchemeFamiliarity()
    {
        var prior = Prior();
        var record = TeamChemistrySeasonService.Advance(prior, Inputs());

        Assert.True(record.Cohesion > prior.Cohesion);
        Assert.True(record.SchemeFamiliarity > prior.SchemeFamiliarity);
        Assert.Equal(CohesionMomentum.Rising, record.Momentum);
        Assert.Contains(record.Notes, note => note.Contains("Head coach continuity"));
        Assert.Contains(record.Notes, note => note.Contains("same game plan"));
        Assert.Contains(record.Notes, note => note.Contains("captain returned"));
    }

    [Fact]
    public void LosingEveryCaptainCostsCohesion()
    {
        var prior = Prior();
        var record = TeamChemistrySeasonService.Advance(prior, Inputs() with { CaptainIds = [], CaptainNames = [] });

        Assert.True(record.Cohesion < prior.Cohesion);
        Assert.Contains(record.Notes, note => note.Contains("No captains returned"));
    }

    [Fact]
    public void ChangedPlanCutsIntoSchemeFamiliarity()
    {
        var prior = Prior();
        var record = TeamChemistrySeasonService.Advance(prior, Inputs() with { PlanKey = "PowerRun/Base" });

        Assert.True(record.SchemeFamiliarity < prior.SchemeFamiliarity);
        Assert.Contains(record.Notes, note => note.Contains("changed game plan"));
    }

    [Fact]
    public void AdvanceIsDeterministicForTheSameInputs()
    {
        var prior = Prior();
        var first = TeamChemistrySeasonService.Advance(prior, Inputs());
        var second = TeamChemistrySeasonService.Advance(prior, Inputs());

        Assert.Equal(first.TeamId, second.TeamId);
        Assert.Equal(first.Cohesion, second.Cohesion);
        Assert.Equal(first.SchemeFamiliarity, second.SchemeFamiliarity);
        Assert.Equal(first.Momentum, second.Momentum);
        Assert.Equal(first.Notes, second.Notes);
        Assert.Equal(first.Captains, second.Captains);
    }

    [Fact]
    public void SeasonMemoryAppliesABoundedOffset()
    {
        Assert.Equal(60, TeamChemistrySeasonService.ApplySeasonMemory(null, 60));
        Assert.Equal(TeamChemistrySeasonService.Maximum, TeamChemistrySeasonService.ApplySeasonMemory(null, 99));

        var record = new TeamChemistryRecord
        {
            TeamName = "Washington",
            SeasonYear = 2027,
            Cohesion = 65,
            EvaluatedCohesion = 60
        };

        Assert.Equal(65, TeamChemistrySeasonService.ApplySeasonMemory(record, 60));
        Assert.Equal(TeamChemistrySeasonService.Maximum, TeamChemistrySeasonService.ApplySeasonMemory(record, 99));
        Assert.Equal(TeamChemistrySeasonService.Minimum, TeamChemistrySeasonService.ApplySeasonMemory(record, 1));
    }

    [Fact]
    public void LockerRoomEventsAreDeterministicAndUseRecordedFactsOnly()
    {
        var signals = new[]
        {
            new LockerRoomSignal
            {
                TeamName = "Washington",
                Kind = LockerRoomEventKind.CoachingChange,
                Subject = "Jane Doe",
                SeasonYear = 2027,
                Week = 0
            },
            new LockerRoomSignal
            {
                TeamName = "Washington",
                Kind = LockerRoomEventKind.RosterTurnover,
                Value = 30,
                SeasonYear = 2027,
                Week = 0
            }
        };

        var first = LockerRoomEventService.Build(signals);
        var second = LockerRoomEventService.Build(signals);

        Assert.Equal(first, second);
        Assert.Equal(2, first.Count);
        Assert.Contains(first, item => item.Headline == "New head coach" && item.Detail.Contains("Jane Doe"));
        Assert.Contains(first, item => item.Kind == LockerRoomEventKind.RosterTurnover && item.Detail.Contains("30%"));
        Assert.All(first, item => Assert.StartsWith("locker/", item.Id));
    }

    static TeamChemistryRecord Prior() => TeamChemistrySeasonService.Advance(null, Inputs()) with
    {
        Cohesion = 62,
        SchemeFamiliarity = 60,
        Momentum = CohesionMomentum.Stable
    };

    static TeamChemistryInputs Inputs() => new()
    {
        TeamName = "Washington",
        SeasonYear = 2027,
        EvaluatedCohesion = 60,
        EvaluatedSchemeFamiliarity = 58,
        HeadCoachKey = "coach-1",
        PlanKey = "Balanced/Base",
        CaptainIds = [CaptainId],
        CaptainNames = ["Test Captain"]
    };
}