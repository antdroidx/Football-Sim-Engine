using ProFootball.Core;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;
namespace ProFootball.Tests;
public sealed class StaffPhilosophyTests
{
    [Theory]
    [InlineData("Spread", OffensivePlaybook.Spread)]
    [InlineData("Spread play action", OffensivePlaybook.Spread)]
    [InlineData("Play-action spread", OffensivePlaybook.PlayAction)]
    [InlineData("RPO and play-action spread", OffensivePlaybook.PlayAction)]
    [InlineData("Power run play action", OffensivePlaybook.PowerRun)]
    [InlineData("Power spread", OffensivePlaybook.PowerRun)]
    [InlineData("Play-action balanced", OffensivePlaybook.PlayAction)]
    [InlineData("West Coast play action", OffensivePlaybook.PlayAction)]
    [InlineData("Balanced play action", OffensivePlaybook.Balanced)]
    [InlineData("Erhardt-Perkins multiple", OffensivePlaybook.Balanced)]
    [InlineData("Air raid", OffensivePlaybook.AirRaid)]
    public void OffenseLabelsMapToPlaybooks(string label, OffensivePlaybook expected) =>
        Assert.Equal(expected, ProTeamPhilosophy.ParseOffense(label));

    [Theory]
    [InlineData("3-4 multiple", DefensivePlaybook.ThreeFour)]
    [InlineData("4-3 quarters", DefensivePlaybook.FourThree)]
    [InlineData("4-3", DefensivePlaybook.FourThree)]
    [InlineData("Nickel coverage", DefensivePlaybook.Nickel)]
    public void DefenseLabelsMapToPlaybooks(string label, DefensivePlaybook expected) =>
        Assert.Equal(expected, ProTeamPhilosophy.ParseDefense(label));

    [Theory]
    [InlineData("", "", 0)]
    [InlineData(null, "Multiple fronts", 0)]
    [InlineData("", "3-4 pressure", 1)]
    [InlineData("West Coast play action", "Multiple fronts", 1)]
    public void InfluenceRequiresAnEngineMappableLabel(string? offense, string? defense, int expectedSign)
    {
        var head = new ProCoach("SEA", StaffRole.HeadCoach, "Coach", "Head coach", "test") { TenureYears = 4, Leadership = 70, PreferredOffense = offense ?? "", PreferredDefense = defense ?? "" };
        var influence = ProTeamPhilosophy.Influence(head);
        if (expectedSign <= 0) Assert.Equal(0d, influence); else Assert.True(influence > 0);
    }

    [Fact]
    public void SeasonZeroStaffKeepsCatalogIdentity()
    {
        var s = ProLeagueService.Create("Seattle Seahawks", preserveLegacyPracticeForAudit: true);
        var identity = ProRosterPlanService.GetIdentity(s, s.UserTeam);
        Assert.Null(identity.StaffPhilosophy);
        Assert.Equal(5, ProRosterPlanService.GetPositionTarget(s, Position.WR, s.UserTeam));
    }

    [Fact]
    public void PhilosophyBlendsTargetsTowardCoachScheme()
    {
        var s = ProLeagueService.Create("Seattle Seahawks", preserveLegacyPracticeForAudit: true);
        var club = s.League.Teams.First(t => t.Name == s.UserTeam);
        var head = s.Coaches.First(c => c.TeamId == club.Id && c.Role == StaffRole.HeadCoach && !c.Vacant);
        s = s with { Coaches = s.Coaches.Select(c => c == head ? c with { PreferredOffense = "Air raid", PreferredDefense = "Nickel coverage", TenureYears = 8 } : c).ToList() };
        var identity = ProRosterPlanService.GetIdentity(s, s.UserTeam);
        Assert.NotNull(identity.StaffPhilosophy);
        // Base play-action target 5 blends toward the air-raid 7 by the staff influence weight.
        Assert.InRange(ProRosterPlanService.GetPositionTarget(s, Position.WR, s.UserTeam), 6, 7);
        // Nickel targets blend cornerbacks upward from the base 4-3 count.
        Assert.InRange(ProRosterPlanService.GetPositionTarget(s, Position.CB, s.UserTeam), 4, 5);
        var basePriorities = RosterPlanService.GetPositionPriorities(OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Balanced);
        Assert.True(identity.PositionPriorities[Position.WR] > basePriorities[Position.WR]);
        // The saved plan is never rewritten by identity adaptation.
        Assert.Equal(s.GamePlans.Last(p => p.TeamName == s.UserTeam).Offense, ProTeamStrategyCatalog.Get(club.Id).Offense);
    }

    [Fact]
    public void PlanAuthoritySurvivesUserOverridesWithoutStaffChange()
    {
        var s = ProLeagueService.Create("Seattle Seahawks", preserveLegacyPracticeForAudit: true);
        var plan = new TeamGamePlan { TeamName = s.UserTeam, Offense = OffensivePlaybook.AirRaid, Defense = DefensivePlaybook.Nickel };
        s = s with { GamePlans = [plan] };
        // Coaches still prefer the club baseline, so the user plan stays authoritative.
        Assert.Equal(7, ProRosterPlanService.GetPositionTarget(s, Position.WR, s.UserTeam));
        Assert.Null(ProRosterPlanService.GetIdentity(s, s.UserTeam).StaffPhilosophy);
    }

    [Fact]
    public void HeadCoachHiresCarryPersonalPhilosophy()
    {
        var knownOffense = new[] { "Spread motion", "West Coast play action", "Play-action spread", "Air raid", "Multiple play action", "Balanced play action", "Erhardt-Perkins multiple" };
        var knownDefense = new[] { "3-4 pressure", "4-3 quarters", "Multiple fronts", "Nickel coverage", "3-4 multiple" };
        var s = ProLeagueService.Create("Seattle Seahawks", preserveLegacyPracticeForAudit: true) with { CoachCandidates = [new("Nolan Brooks", "HC", "Coordinator · experienced", "staff pool")] };
        s = ProStaff.Hire(s, s.UserTeam, StaffRole.HeadCoach, "Nolan Brooks");
        var club = s.League.Teams.Single(t => t.Name == s.UserTeam);
        var hired = s.Coaches.Single(c => c.TeamId == club.Id && c.Role == StaffRole.HeadCoach);
        Assert.Contains(hired.PreferredOffense, knownOffense);
        Assert.Contains(hired.PreferredDefense, knownDefense);
        // The offense label always maps so identity priorities can follow hired coaches.
        Assert.NotNull(ProTeamPhilosophy.ParseOffense(hired.PreferredOffense));
        // Rehiring the same coach in the same franchise is stable.
        s = ProStaff.Fire(s, s.UserTeam, StaffRole.HeadCoach);
        s = s with { CoachCandidates = s.CoachCandidates.Append(new("Nolan Brooks", "HC", "Coordinator · experienced", "staff pool")).ToList() };
        s = ProStaff.Hire(s, s.UserTeam, StaffRole.HeadCoach, "Nolan Brooks");
        var rehired = s.Coaches.Single(c => c.TeamId == club.Id && c.Role == StaffRole.HeadCoach);
        Assert.Equal(hired.PreferredOffense, rehired.PreferredOffense);
        Assert.Equal(hired.PreferredDefense, rehired.PreferredDefense);
    }

    [Fact]
    public void CoordinatorsKeepClubBaselineSchemes()
    {
        var s = ProLeagueService.Create("Seattle Seahawks", preserveLegacyPracticeForAudit: true) with { CoachCandidates = [new("Nolan Brooks", "HC", "Coordinator · experienced", "staff pool")] };
        var club = s.League.Teams.Single(t => t.Name == s.UserTeam);
        s = ProStaff.Fire(s, s.UserTeam, StaffRole.OffensiveCoordinator);
        s = ProStaff.Hire(s, s.UserTeam, StaffRole.OffensiveCoordinator, "Nolan Brooks");
        var coordinator = s.Coaches.Single(c => c.TeamId == club.Id && c.Role == StaffRole.OffensiveCoordinator);
        var baseline = ProTeamStrategyCatalog.Get(club.Id).OffenseIdentity;
        Assert.Equal(baseline, coordinator.PreferredOffense);
    }
}
