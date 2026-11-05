using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class TeamChemistryTests
{
    [Fact]
    public void CaptainsHaveStableDistinctIds()
    {
        var initial = TeamChemistryService.Initialize("SEA", 2026);
        var modified = TeamChemistryService.WithCaptions(initial, ["qb", "qb", "rb"]);

        Assert.NotEqual(initial.Captains, modified.Captains);
        Assert.Equal(2, modified.Captains.Length);
        Assert.Distinct(modified.Captains);
        Assert.Equal("Rising", modified.Momentum.ToString());
    }

    [Fact]
    public void EvaluateProducesCaptainsAndBoundedCultureValues()
    {
        var captain = Strong();
        var roster = new[] { captain, Weak() };
        var profile = new TeamSimulationProfile
        {
            TeamName = "Seattle Seahawks", Morale = 80, HeadCoachLeadership = 80,
            OffensiveCoordinatorScheme = 85, DefensiveCoordinatorScheme = 70
        };
        var record = TeamChemistryService.Evaluate("SEA", 2026, roster, profile);

        Assert.Contains(captain.PlayerId, record.Captains);
        Assert.InRange(record.Cohesion, 20, 95);
        Assert.InRange(record.SchemeFamiliarity, 20, 95);
        Assert.Equal(CohesionMomentum.Rising, record.Momentum);
    }

    [Fact]
    public void GameShiftIsBoundedAndFavorsTheBetterCulture() =>
        Assert.InRange(TeamChemistryService.GameShift(
            new TeamSimulationProfile { TeamName = "A", Cohesion = 90, SchemeFamiliarity = 90 },
            new TeamSimulationProfile { TeamName = "B", Cohesion = 50, SchemeFamiliarity = 50 }),
            -1.8, 1.8);

    static DynastyPlayer Strong() => New("Strong", 95, 95, 95, 95, 95, 95);
    static DynastyPlayer Weak() => New("Weak", 40, 40, 40, 40, 40, 40);
    static DynastyPlayer New(string name, int overall, int speed, int strength, int agility,
        int awareness, int injury) => new DynastyPlayer
    {
        PlayerId = Guid.NewGuid(), FullName = name, TeamName = "Seattle Seahawks",
        Position = Position.QB, OverallRating = overall, PotentialRating = overall,
        SpeedRating = speed, StrengthRating = strength, AgilityRating = agility,
        AwarenessRating = awareness, InjuryRating = injury, Skills = new()
        { PassingPower = overall, PassingAccuracy = overall, Catching = overall,
          BreakTackle = overall, Tackling = overall, RunBlocking = overall,
          PassBlocking = overall, BallCarrying = overall, Acceleration = overall,
          Leadership = 95, Discipline = 95, Jumping = overall, KickPower = overall,
          KickAccuracy = overall }, DepthChartOrder = 1
    };
}
