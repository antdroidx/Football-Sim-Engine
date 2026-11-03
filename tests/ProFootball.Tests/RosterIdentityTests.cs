using ProFootball.Core;
using DynastyManager.Core.Models;
using Xunit;
using Xunit.Abstractions;
namespace ProFootball.Tests;
public sealed class RosterIdentityTests
{
    private readonly ITestOutputHelper _output;
    public RosterIdentityTests(ITestOutputHelper output) => _output = output;
    [Theory]
    [InlineData(OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, 7, 0, 5)]
    [InlineData(OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, 4, 1, 4)]
    public void ProIdentityUsesSchemePriorityAndTargets(
        OffensivePlaybook offense, DefensivePlaybook defense, int wrTarget, int fbTarget, int edgeTarget)
    {
        var plan = new TeamGamePlan { TeamName="Seattle Seahawks", Offense=offense, Defense=defense };
        var s = ProLeagueService.Create("Seattle Seahawks",preserveLegacyPracticeForAudit:true) with {GamePlans=[plan]};
        _output.WriteLine($"identity {ProRosterPlanService.GetIdentity(s,s.UserTeam).Offense}/{ProRosterPlanService.GetIdentity(s,s.UserTeam).Defense}");
        Assert.Equal(ProRosterPlanService.GetIdentity(s,s.UserTeam).Offense, offense);
        Assert.Equal(ProRosterPlanService.GetIdentity(s,s.UserTeam).Defense, defense);
        Assert.Equal(wrTarget, ProRosterPlanService.GetPositionTarget(s,Position.WR,s.UserTeam));
        Assert.Equal(fbTarget, ProRosterPlanService.GetPositionTarget(s,Position.FB,s.UserTeam));
        Assert.Equal(edgeTarget, ProRosterPlanService.GetPositionTarget(s,Position.OLB,s.UserTeam));
        Assert.True(ProRosterPlanService.GetPositionPriorities(s,s.UserTeam,Position.WR) >
            ProRosterPlanService.GetPositionPriorities(s,s.UserTeam,Position.K));
    }
    [Fact]
    public void ProNeedTreatsSchemePositionsAndRosterAsPlanInputs()
    {
        var plan = new TeamGamePlan { TeamName="Seattle Seahawks", Offense=OffensivePlaybook.AirRaid, Defense=DefensivePlaybook.Nickel };
        var s = ProLeagueService.Create("Seattle Seahawks",preserveLegacyPracticeForAudit:true);
        s = s with { GamePlans = [plan], Players = s.Players.Where(p => p.Football.TeamName != s.UserTeam || p.Football.Position != Position.WR).ToList() };
        _output.WriteLine($"identity {ProRosterPlanService.GetIdentity(s,s.UserTeam).Offense}/{ProRosterPlanService.GetIdentity(s,s.UserTeam).Defense}");
        _output.WriteLine("target " + ProRosterPlanService.GetPositionTarget(s,Position.WR,s.UserTeam));
        _output.WriteLine("active " + ProRosterPlanService.Active(s,s.UserTeam).Count(p => p.Football.Position == Position.WR));
        _output.WriteLine("need " + ProRosterPlanService.Need(s,s.UserTeam,Position.WR));
        Assert.Equal(7, ProRosterPlanService.GetPositionTarget(s,Position.WR,s.UserTeam));
        Assert.Equal(0, ProRosterPlanService.Active(s,s.UserTeam).Count(p => p.Football.Position == Position.WR));
        Assert.Equal(7, ProRosterPlanService.Need(s,s.UserTeam,Position.WR));
        Assert.Equal(0, ProRosterPlanService.Need(s,s.UserTeam,Position.FB));
    }
    [Fact]
    public void ProPlanScoresNeedAndFitBeforeTalent()
    {
        var plan = new TeamGamePlan { TeamName="Seattle Seahawks", Offense=OffensivePlaybook.AirRaid, Defense=DefensivePlaybook.Nickel };
        var s = ProLeagueService.Create("Seattle Seahawks",preserveLegacyPracticeForAudit:true) with {GamePlans=[plan]};
        var roster = ProRosterPlanService.CreatePlan(s,s.UserTeam);
        var receiver = s.Players.First(p => p.Football.TeamName=="Free agent" && p.Football.Position==Position.WR);
        var kicker = s.Players.First(p => p.Football.TeamName=="Free agent" && p.Football.Position==Position.K);
        _output.WriteLine($"wr {roster.Priority(receiver)} kicker {roster.Priority(kicker)}");
        Assert.True(roster.Priority(receiver) > roster.Priority(kicker));
    }
}

