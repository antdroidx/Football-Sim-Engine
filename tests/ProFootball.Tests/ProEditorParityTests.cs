using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

[Trait("Owner", "Pro")]
[Trait("Category", "Fast")]
public sealed class ProEditorParityTests
{
    [Fact]
    public void CoachPortraitCustomizationRoundTripsWithStableIdentity()
    {
        var coach = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false)
            .Coaches.First(c => c.Role == StaffRole.HeadCoach && !c.Vacant);
        var customized = coach with { Appearance = new NcaaPlayerAppearance { HairColor = 2, HairStyle = 6, FacialHair = 3 } };
        var loaded = JsonSerializer.Deserialize<ProCoach>(JsonSerializer.Serialize(customized))!;
        Assert.Equal(coach.Name, loaded.Name);
        Assert.Equal(coach.TeamId, loaded.TeamId);
        Assert.Equal(customized.Appearance, loaded.Appearance);
    }

    [Fact]
    public void CustomPlayLibraryIsValidatedAndAvailableToTheEngine()
    {
        var source = PlaybookCatalog.OffensivePlays.First();
        var custom = source with { Id = "custom-parity", Name = "Parity Call" };
        var plan = new TeamGamePlan
        {
            TeamName = "Seattle Seahawks", Offense = OffensivePlaybook.Custom,
            CustomOffensivePlayIds = [custom.Id], CreatedOffensivePlays = [custom]
        };
        PlayEditorService.Validate(plan);
        Assert.Contains(GamePlanService.AvailableOffensivePlays(plan), play => play.Id == custom.Id);
    }

}
