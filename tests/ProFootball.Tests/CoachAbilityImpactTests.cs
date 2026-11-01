using DynastyManager.Core.Models;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class CoachAbilityImpactTests
{
    static ProFranchise WithAbilities(string team, int offense, int defense, int leadership, int development)
    {
        var s = ProLeagueService.Create(team, generatedPlayers: true, applyOpeningTrades: false);
        var club = s.League.Teams.Single(t => t.Name == team);
        return s with
        {
            Coaches = s.Coaches.Select(c => c.TeamId != club.Id ? c : c.Role switch
            {
                StaffRole.OffensiveCoordinator => ProStaff.Apply(c, ProStaff.Profile(c).Attributes with {
                    OffensePlaycalling = offense, Development = development, OffenseDevelopment = development }),
                StaffRole.DefensiveCoordinator => ProStaff.Apply(c, ProStaff.Profile(c).Attributes with {
                    DefenseScheme = defense, Development = development, DefenseDevelopment = development }),
                StaffRole.HeadCoach => ProStaff.Apply(c, ProStaff.Profile(c).Attributes with {
                    Leadership = leadership, Development = development, OffenseDevelopment = development,
                    DefenseDevelopment = development }),
                _ => c
            }).ToList()
        };
    }

    [Fact]
    public void CoordinatorAbilitiesMoveSimulationRatings()
    {
        const string team = "Seattle Seahawks";
        var weak = WithAbilities(team, 35, 35, 35, 50);
        var strong = WithAbilities(team, 95, 95, 95, 90);
        Assert.True(ProLeagueService.Profile(strong, team).PassOffenseRating > ProLeagueService.Profile(weak, team).PassOffenseRating);
        Assert.True(ProLeagueService.Profile(strong, team).PassDefenseRating > ProLeagueService.Profile(weak, team).PassDefenseRating);
        Assert.True(ProLeagueService.Profile(strong, team).HeadCoachLeadership > ProLeagueService.Profile(weak, team).HeadCoachLeadership);
    }

    [Fact]
    public void StaffDevelopmentRatingFollowsUnitCoaches()
    {
        const string team = "Seattle Seahawks";
        var weak = WithAbilities(team, 60, 60, 60, 40);
        var strong = WithAbilities(team, 60, 60, 60, 92);
        Assert.True(ProStaff.DevelopmentRating(strong, team, Position.QB) > ProStaff.DevelopmentRating(weak, team, Position.QB));
        Assert.True(ProStaff.DevelopmentRating(strong, team, Position.DE) > ProStaff.DevelopmentRating(weak, team, Position.DE));
    }
}