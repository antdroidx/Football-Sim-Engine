using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;
namespace ProFootball.Tests;
public sealed class PersonnelAdaptationTests
{
    [Theory]
    [InlineData(true, OffensivePlaybook.Spread)]
    [InlineData(false, OffensivePlaybook.PowerRun)]
    public void PersonnelShapeLeansOffense(bool passHeavy, OffensivePlaybook expected)
    {
        var shapes = new PersonnelProfile(passHeavy ? 1.6 : .5, passHeavy ? .5 : 1.6, 1, 1);
        var (offense, _) = RosterPlanService.AdaptToPersonnel(
            passHeavy ? OffensivePlaybook.PowerRun : OffensivePlaybook.AirRaid,
            DefensivePlaybook.Nickel,
            shapes);
        Assert.Equal(expected, offense);
    }
    [Theory]
    [InlineData(true, DefensivePlaybook.ThreeFour)]
    [InlineData(false, DefensivePlaybook.Nickel)]
    public void PersonnelShapeLeansDefense(bool edgeHeavy, DefensivePlaybook expected)
    {
        var shapes = new PersonnelProfile(1, 1, edgeHeavy ? .5 : 1.6, edgeHeavy ? 1.6 : .5);
        var (_, defense) = RosterPlanService.AdaptToPersonnel(
            OffensivePlaybook.PlayAction,
            edgeHeavy ? DefensivePlaybook.Nickel : DefensivePlaybook.ThreeFour,
            shapes);
        Assert.Equal(expected, defense);
    }
    [Fact]
    public void BalancedShapeKeepsSchemes()
    {
        var shapes = new PersonnelProfile(1, 1, 1, 1);
        var (offense, defense) = RosterPlanService.AdaptToPersonnel(OffensivePlaybook.PlayAction, DefensivePlaybook.Nickel, shapes);
        Assert.Equal(OffensivePlaybook.PlayAction, offense);
        Assert.Equal(DefensivePlaybook.Nickel, defense);
    }
}
