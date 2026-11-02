using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
[Trait("Category", "Fast")]
public sealed class CanonicalAdapterTests
{
    [Theory]
    [InlineData(Position.K, 19)] [InlineData(Position.P, 20)] [InlineData(Position.FB, 2)]
    public void SpecialistAndFullbackAdaptersPreserveImportedIdentityAndMeasurements(Position target, int code)
    {
        var imported = new DynastyPlayer { PlayerId = Guid.NewGuid(), FullName = "Imported player", TeamName = "A",
            Position = Position.QB, OverallRating = 80, PotentialRating = 90, AwarenessRating = 75,
            HeightInches = 76, WeightPounds = 225, ClassYear = 3 };
        var converted = PlayerPositionConversion.Convert(imported, target);
        Assert.Equal(imported.PlayerId, converted.PlayerId); Assert.Equal(imported.TeamName, converted.TeamName);
        Assert.Equal(imported.ClassYear, converted.ClassYear); Assert.Equal(code, converted.NcaaPositionCode);
        Assert.Equal(imported.HeightInches, converted.HeightInches); Assert.Equal(imported.WeightPounds, converted.WeightPounds);
        Assert.True(converted.PotentialRating >= converted.OverallRating);
        var measured = PlayerPhysicalService.EnsureMeasurements(converted);
        Assert.Equal(converted, measured);
        var missing = converted with { HeightInches = 0 };
        var filled = PlayerPhysicalService.EnsureMeasurements(missing);
        Assert.InRange(filled.HeightInches, 65, 85); Assert.Equal(225, filled.WeightPounds);
        Assert.Equal(filled, PlayerPhysicalService.EnsureMeasurements(filled));
    }
}
