using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;
using static Football.Testing.GameDayFixture;
namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
public sealed class ModeConformanceTests
{
    [Theory]
    [Trait("Category", "Balance")]
    [InlineData(false, 7123)] [InlineData(false, 19)] [InlineData(false, 41)]
    [InlineData(false, 73)] [InlineData(false, 103)] [InlineData(false, 211)]
    [InlineData(true, 7123)] [InlineData(true, 19)] [InlineData(true, 41)]
    [InlineData(true, 73)] [InlineData(true, 103)] [InlineData(true, 211)]
    public void SimWatchAndCpuDelegatedPlayProduceTheSameStats(bool professional, int seed)
    {
        var source = Session(GameDayMode.Simulate, includeOpeningKickoff: true, seed: seed);
        var games = new List<ScheduledGame>();
        var events = new List<GameSnapEvent[]>();
        foreach (var mode in Enum.GetValues<GameDayMode>())
        {
            var session = GameDayEngine.Start(source.Game, source.HomeProfile, source.AwayProfile, source.Roster, mode,
                rules: professional ? FootballRules.Professional : FootballRules.College);
            // No user calls or controlled school: all three modes delegate identical decisions to CPU.
            GameDayEngine.SimulateToEnd(session);
            games.Add(GameDayEngine.Finish(session));
            events.Add(session.Events.ToArray());
        }
        foreach (var other in events.Skip(1))
        {
            Assert.Equal(events[0].Length, other.Length);
            foreach (var (expected, actual) in events[0].Zip(other))
            {
                Assert.Equal(expected.Outcome, actual.Outcome);
                Assert.Equal(expected.Quarter, actual.Quarter);
                Assert.Equal(expected.ClockSeconds, actual.ClockSeconds);
                Assert.Equal(expected.OffensePersonnel, actual.OffensePersonnel);
                Assert.Equal(expected.DefensePersonnel, actual.DefensePersonnel);
                Assert.Equal(expected.PreSnapSeconds, actual.PreSnapSeconds);
                Assert.Equal(expected.LiveBallSeconds, actual.LiveBallSeconds);
            }
        }
        foreach (var game in games.Skip(1))
        {
            Assert.Equal(games[0].HomeScore, game.HomeScore);
            Assert.Equal(games[0].AwayStats, game.AwayStats);
            Assert.Equal(games[0].HomeStats, game.HomeStats);
            Assert.Equal(games[0].PlayerStats.OrderBy(p => p.PlayerId), game.PlayerStats.OrderBy(p => p.PlayerId));
        }
    }

}
