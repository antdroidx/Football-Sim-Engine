using System;
using System.Threading;
using System.Threading.Tasks;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class GameDayPreSnapPresentationTests
{
    [Fact]
    public async Task AlignmentFollowsTheFormationAndHoldsForTheFormationLead()
    {
        var calls = new List<string>();

        await GameDayPreSnapPresentation.HoldAsync(
            true,
            () => Record("formation"),
            () => Record("alignment"),
            (delay, _) => Record($"delay:{delay.TotalMilliseconds:0}"));

        Assert.Equal(["formation", "delay:3500", "delay:180", "alignment", "delay:1000"], calls);

        Task Record(string value)
        {
            calls.Add(value);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DefensiveAlignmentRunsBeforeOffensiveMotion()
    {
        var calls = new List<string>();

        await GameDayPreSnapPresentation.HoldSequenceAsync(
            true,
            () => Record("formation"),
            () => Record("defense"),
            () => Record("offense"),
            (delay, _) => Record($"delay:{delay.TotalMilliseconds:0}"));

        Assert.Equal(["formation", "delay:3500", "delay:180", "defense", "offense", "delay:1000"], calls);

        Task Record(string value)
        {
            calls.Add(value);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task LiveJetMotionStartsLateEnoughToArriveAtTheSnap()
    {
        var calls = new List<string>();

        await GameDayPreSnapPresentation.HoldSequenceAsync(
            true,
            () => Record("formation"),
            () => Record("defense"),
            () => Record("motion-to-mesh"),
            (delay, _) => Record($"delay:{delay.TotalMilliseconds:0}"),
            settleAfterAlignment: false,
            motionLiveAtSnap: true);

        Assert.Equal([
            "formation", "delay:3500", "delay:2720", "delay:180", "defense",
            "motion-to-mesh"
        ], calls);

        Task Record(string value)
        {
            calls.Add(value);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PlainFormationStillWaitsForTheThreeAndOneHalfSecondLead()
    {
        var calls = new List<string>();

        await GameDayPreSnapPresentation.HoldAsync(
            false,
            () => Record("formation"),
            () => Record("alignment"),
            (delay, _) => Record($"delay:{delay.TotalMilliseconds:0}"));

        Assert.Equal(["formation", "delay:3500"], calls);
        Assert.Equal(TimeSpan.FromSeconds(3.5), GameDayPreSnapPresentation.FormationLead);

        Task Record(string value)
        {
            calls.Add(value);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SnapCannotProceedUntilTheFormationLeadCompletes()
    {
        var formationLead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sequence = GameDayPreSnapPresentation.HoldSequenceAsync(
            false,
            () => Task.CompletedTask,
            () => Task.CompletedTask,
            () => Task.CompletedTask,
            (delay, _) => delay == GameDayPreSnapPresentation.FormationLead
                ? formationLead.Task
                : Task.CompletedTask);

        await Task.Yield();
        Assert.False(sequence.IsCompleted);

        formationLead.SetResult();
        await sequence;
        Assert.True(sequence.IsCompletedSuccessfully);
    }
    [Fact]
    public async Task JetSnapWaitsForBothTheFormationLeadAndMotionAnimation()
    {
        var formationLead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var motionFinished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var motionStarted = false;
        var sequence = GameDayPreSnapPresentation.HoldSequenceAsync(
            true,
            () => Task.CompletedTask,
            () => Task.CompletedTask,
            () => { motionStarted = true; return motionFinished.Task; },
            (delay, _) => delay == GameDayPreSnapPresentation.FormationLead
                ? formationLead.Task
                : Task.CompletedTask,
            settleAfterAlignment: false,
            motionLiveAtSnap: true);

        await Task.Yield();
        Assert.True(motionStarted);
        Assert.False(sequence.IsCompleted);
        motionFinished.SetResult();
        await Task.Yield();
        Assert.False(sequence.IsCompleted);
        formationLead.SetResult();
        await sequence;
    }

}
