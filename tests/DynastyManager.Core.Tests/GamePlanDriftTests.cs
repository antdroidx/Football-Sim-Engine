using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public sealed class GamePlanDriftTests
{
    private static DynastyState State() => new()
    {
        DynastyId = Guid.NewGuid(),
        DynastyName = "Drift",
        UserTeamName = "User",
        SeasonYear = 2026,
        LeagueTeams =
        [
            new Team { Name = "CPU", Abbreviation = "CPU", ConferenceName = "Test" },
            new Team { Name = "User", Abbreviation = "USR", ConferenceName = "Test" }
        ],
        TeamGamePlans =
        [
            new TeamGamePlan { TeamName = "CPU", Offense = OffensivePlaybook.Balanced, Defense = DefensivePlaybook.FourThree },
            new TeamGamePlan { TeamName = "User", Offense = OffensivePlaybook.AirRaid, Defense = DefensivePlaybook.Nickel, Tempo = OffensiveTempo.UpTempo }
        ]
    };

    private static StaffMember Head(int offense, int defense, int tenure = 4, int leadership = 80) => new()
    {
        FullName = "Head Coach",
        TeamName = "CPU",
        Role = StaffRole.HeadCoach,
        TenureYears = tenure,
        Leadership = leadership,
        Coaching = new CoachingAttributes { OffensePlaycalling = offense, DefenseScheme = defense, Leadership = leadership }
    };

    [Fact]
    public void StepsFollowTheAdjacentSchemeGraph()
    {
        Assert.Equal(OffensivePlaybook.Spread, RosterPlanService.OffenseStep(OffensivePlaybook.Balanced, OffensivePlaybook.AirRaid));
        Assert.Equal(OffensivePlaybook.AirRaid, RosterPlanService.OffenseStep(OffensivePlaybook.Spread, OffensivePlaybook.AirRaid));
        Assert.Equal(OffensivePlaybook.PowerRun, RosterPlanService.OffenseStep(OffensivePlaybook.Balanced, OffensivePlaybook.PowerRun));
        Assert.Equal(OffensivePlaybook.Balanced, RosterPlanService.OffenseStep(OffensivePlaybook.Balanced, OffensivePlaybook.Balanced));
        Assert.Equal(OffensivePlaybook.Custom, RosterPlanService.OffenseStep(OffensivePlaybook.Custom, OffensivePlaybook.Spread));
        Assert.Equal(DefensivePlaybook.FourThree, RosterPlanService.DefenseStep(DefensivePlaybook.Balanced, DefensivePlaybook.Nickel));
        Assert.Equal(DefensivePlaybook.Nickel, RosterPlanService.DefenseStep(DefensivePlaybook.FourThree, DefensivePlaybook.Nickel));
        Assert.Equal(DefensivePlaybook.ThreeFour, RosterPlanService.DefenseStep(DefensivePlaybook.Nickel, DefensivePlaybook.ThreeFour));
        Assert.Equal(DefensivePlaybook.Balanced, RosterPlanService.DefenseStep(DefensivePlaybook.Balanced, DefensivePlaybook.Balanced));
        Assert.Equal(DefensivePlaybook.Custom, RosterPlanService.DefenseStep(DefensivePlaybook.Custom, DefensivePlaybook.Nickel));
    }

    [Fact]
    public void EstablishedOffensiveHeadDriftsTheCpuPlanOneStepPerSeason()
    {
        var state = State() with { Staff = [Head(offense: 82, defense: 60)] };
        var advanced = StaffManagementService.AdvanceSeason(state);
        var plan = advanced.TeamGamePlans.Single(item => item.TeamName == "CPU");
        Assert.Equal(OffensivePlaybook.Spread, plan.Offense);
        Assert.Equal(DefensivePlaybook.FourThree, plan.Defense);
        Assert.Equal(OffensivePlaybook.AirRaid, advanced.TeamGamePlans.Single(item => item.TeamName == "User").Offense);
        Assert.Same(advanced, StaffManagementService.AdvanceSeason(advanced));
    }

    [Fact]
    public void DriftNeverOvershootsAndStopsAtTheTarget()
    {
        var state = State() with
        {
            TeamGamePlans = [
                new TeamGamePlan { TeamName = "CPU", Offense = OffensivePlaybook.Spread, Defense = DefensivePlaybook.Nickel }
            ],
            Staff = [Head(offense: 82, defense: 60)] // Lean targets Spread; the plan is already there.
        };
        var advanced = StaffManagementService.AdvanceSeason(state);
        var plan = advanced.TeamGamePlans.Single(item => item.TeamName == "CPU");
        Assert.Equal(OffensivePlaybook.Spread, plan.Offense);
        Assert.Equal(DefensivePlaybook.Nickel, plan.Defense);
    }

    [Fact]
    public void DriftWaitsUntilHeadInfluenceIsEstablished()
    {
        var first = State() with { Staff = [Head(offense: 82, defense: 60, tenure: 0, leadership: 55)] };
        Assert.Equal(OffensivePlaybook.Balanced,
            StaffManagementService.AdvanceSeason(first).TeamGamePlans.Single(item => item.TeamName == "CPU").Offense);
        var settled = State() with { Staff = [Head(offense: 82, defense: 60, tenure: 2, leadership: 55)] };
        Assert.Equal(OffensivePlaybook.Spread,
            StaffManagementService.AdvanceSeason(settled).TeamGamePlans.Single(item => item.TeamName == "CPU").Offense);
    }

    [Fact]
    public void SeasonZeroBalancedStaffKeepsItsPlan()
    {
        var state = State() with { Staff = [Head(offense: 65, defense: 65, tenure: 0, leadership: 60)] };
        var advanced = StaffManagementService.AdvanceSeason(state);
        Assert.Equal(OffensivePlaybook.Balanced, advanced.TeamGamePlans.Single(item => item.TeamName == "CPU").Offense);
    }
}
