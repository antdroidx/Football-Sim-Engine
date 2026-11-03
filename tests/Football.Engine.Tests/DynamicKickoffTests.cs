using Football.Testing;
using static Football.Testing.GameDayFixture;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Category", "Fast")]
[Trait("Owner", "Engine")]
public sealed class DynamicKickoffTests
{
    [Theory]
    [InlineData(35)] [InlineData(20)] [InlineData(50)]
    public void ProductionProKicksRecordBothFlightKindsAndAwardTheCorrectSpot(int from)
    {
        var source = GameDayFixture.Session(GameDayMode.Simulate);
        var kinds = new HashSet<bool>();
        for (var seed = 0; seed < 64; seed++)
        {
            var s = GameDayEngine.Start(source.Game with { SimulationSeed = seed }, source.HomeProfile,
                source.AwayProfile, source.Roster, GameDayMode.Simulate, rules: FootballRules.Professional);
            s.KickoffFromOwnYardLine = from;
            var snap = GameDayEngine.Step(s);
            Assert.True(snap.UsesDynamicKickoff); Assert.Equal(100 - from, snap.StartYardLine);
            Assert.Equal(11, snap.OffensePersonnel.Distinct().Count()); Assert.Equal(11, snap.DefensePersonnel.Distinct().Count());
            if (!snap.IsTouchback) continue;
            kinds.Add(snap.KickTouchedLandingZone);
            Assert.Equal(snap.KickTouchedLandingZone || from == 50 ? 20 : 35, s.YardLine);
            Assert.Equal(900, s.ClockSeconds); Assert.Equal(0, snap.ReturnYards);
            Assert.Null(snap.ReturnerId); Assert.Equal(0, GameDayEngine.BoxScore(s).Players.Sum(p => p.KickReturns));
            var loaded = System.Text.Json.JsonSerializer.Deserialize<GameSnapEvent>(System.Text.Json.JsonSerializer.Serialize(snap))!;
            Assert.Equal(snap.KickTouchedLandingZone, loaded.KickTouchedLandingZone);
        }
        Assert.Equal(2, kinds.Count);
    }
}
