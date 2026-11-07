using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class GameDayPreSnapPresentationTests
{
    [Fact]
    public async Task AlignmentFollowsTheFormationAndSettlesBeforeSnap()
    {
        var calls = new List<string>();

        await GameDayPreSnapPresentation.HoldAsync(
            true,
            () => Record("formation"),
            () => Record("alignment"),
            (delay, _) => Record($"delay:{delay.TotalMilliseconds:0}"));

        Assert.Equal(["formation", "delay:180", "alignment", "delay:1000"], calls);

        Task Record(string value)
        {
            calls.Add(value);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PlainFormationStillHoldsBeforeSnap()
    {
        var calls = new List<string>();

        await GameDayPreSnapPresentation.HoldAsync(
            false,
            () => Record("formation"),
            () => Record("alignment"),
            (delay, _) => Record($"delay:{delay.TotalMilliseconds:0}"));

        Assert.Equal(["formation", "delay:1000"], calls);

        Task Record(string value)
        {
            calls.Add(value);
            return Task.CompletedTask;
        }
    }
}
