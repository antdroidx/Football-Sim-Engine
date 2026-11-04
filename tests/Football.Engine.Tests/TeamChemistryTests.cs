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
}
