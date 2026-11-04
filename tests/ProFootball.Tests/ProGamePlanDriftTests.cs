using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public sealed class ProGamePlanDriftTests
{
    private static ProFranchise New() => ProLeagueService.Create("Seattle Seahawks", preserveLegacyPracticeForAudit: true);

    private static (ProTeamStrategy Baseline, ProFranchise State) Ready(string offense, string defense, int tenure, int leadership)
    {
        var baseline = ProTeamStrategyCatalog.All.First(team =>
            team.Offense is not (OffensivePlaybook.AirRaid or OffensivePlaybook.Custom) &&
            team.Defense is not (DefensivePlaybook.Nickel or DefensivePlaybook.Custom));
        var s = New();
        var club = s.League.Teams.Single(t => t.Id == baseline.TeamId);
        var head = s.Coaches.First(c => c.TeamId == club.Id && c.Role == StaffRole.HeadCoach && !c.Vacant);
        s = s with { Coaches = s.Coaches.Select(c => c == head ? c with
        {
            PreferredOffense = offense,
            PreferredDefense = defense,
            TenureYears = tenure,
            Leadership = leadership,
            Vacant = false
        } : c).ToList() };
        return (baseline, s);
    }

    [Fact]
    public void EstablishedPhilosophyDriftsTheSavedPlanOneAdjacentStep()
    {
        var (baseline, s) = Ready("Air raid", "Nickel coverage", 8, 85);
        var drifted = ProGamePlanDrift.Apply(s);
        var plan = Assert.Single(drifted.GamePlans, p => p.TeamName == baseline.TeamName);
        Assert.Equal(RosterPlanService.OffenseStep(baseline.Offense, OffensivePlaybook.AirRaid), plan.Offense);
        Assert.Equal(RosterPlanService.DefenseStep(baseline.Defense, DefensivePlaybook.Nickel), plan.Defense);
        Assert.Equal(baseline.Approach, plan.Approach);
        Assert.Equal(baseline.Tempo, plan.Tempo);
        var userPlan = drifted.GamePlans.Single(plan => plan.TeamName == s.UserTeam);
        Assert.Equal(s.GamePlans.Single(plan => plan.TeamName == s.UserTeam).Offense, userPlan.Offense);
        Assert.Equal(s.GamePlans.Single(plan => plan.TeamName == s.UserTeam).Defense, userPlan.Defense);
    }

    [Fact]
    public void WeakInfluenceNeverDriftsAndSeasonZeroCatalogIdentityStays()
    {
        var (baseline, s) = Ready("Air raid", "Nickel coverage", 0, 55);
        var drifted = ProGamePlanDrift.Apply(s);
        var before = s.GamePlans.Single(plan => plan.TeamName == baseline.TeamName);
        var after = drifted.GamePlans.Single(plan => plan.TeamName == baseline.TeamName);
        Assert.Equal(before.Offense, after.Offense);
        Assert.Equal(before.Defense, after.Defense);
        Assert.Equal(before.Approach, after.Approach);
    }

    [Fact]
    public void CustomPlaybooksDriftNeitherSide()
    {
        var (baseline, s) = Ready("Air raid", "Nickel coverage", 8, 85);
        var club = s.League.Teams.Single(t => t.Name == baseline.TeamName);
        s = s with { GamePlans = [new TeamGamePlan
        {
            TeamName = baseline.TeamName,
            Offense = OffensivePlaybook.Custom,
            Defense = DefensivePlaybook.Custom,
            Approach = baseline.Approach,
            CustomOffensivePlayIds = ["empty-shortsidescreen"],
            CustomDefensivePlayIds = ["fourthree-stunt"]
        }] };
        var drifted = ProGamePlanDrift.Apply(s);
        var plan = drifted.GamePlans.Single(p => p.TeamName == club.Name);
        Assert.Equal(OffensivePlaybook.Custom, plan.Offense);
        Assert.Equal(DefensivePlaybook.Custom, plan.Defense);
    }
}
