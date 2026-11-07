using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
[Trait("Category", "Fast")]
public sealed class PreSnapMatchupTests
{
    [Fact]
    public void ManAndFocusedCoverageCarryMotionWhileZonePassesTheReceiverOff()
    {
        var play = PlaybookCatalog.OffensivePlays.Single(item => item.Id == "off-empty-motion-cross");
        var motion = play.PreSnapAction!;
        var man = Resolve(motion, "def-43-cover1", new() { Position = Position.CB, RoleIndex = 0, Task = AssignmentTask.Man }, 90, 60);
        var zone = Resolve(motion, "def-43-cover3", new() { Position = Position.CB, RoleIndex = 0, Task = AssignmentTask.Zone }, 90, 60);
        var focused = Resolve(motion, "def-dime-bracket", new() { Position = Position.CB, RoleIndex = 0, Task = AssignmentTask.Zone }, 90, 60);

        Assert.Equal(PreSnapCoverageReaction.ManMatchupHeld, man.Reaction);
        Assert.True(man.DefenderFollowsMotion);
        Assert.False(man.MatchupChanged);

        Assert.Equal(PreSnapCoverageReaction.ZonePassOff, zone.Reaction);
        Assert.False(zone.DefenderFollowsMotion);
        Assert.True(zone.MatchupChanged);

        Assert.Equal(PreSnapCoverageReaction.FocusedMatchupHeld, focused.Reaction);
        Assert.True(focused.DefenderFollowsMotion);
        Assert.False(focused.MatchupChanged);
    }

    [Fact]
    public void CoverageReactionCanHelpOrHurtTheRecordedPlay()
    {
        var motion = PlaybookCatalog.OffensivePlays.Single(item => item.Id == "off-empty-motion-cross").PreSnapAction!;
        var strongZone = Resolve(motion, "def-43-cover3", new() { Position = Position.CB, RoleIndex = 0, Task = AssignmentTask.Zone }, 95, 50);
        var weakZone = Resolve(motion, "def-43-cover3", new() { Position = Position.CB, RoleIndex = 0, Task = AssignmentTask.Zone }, 50, 95);
        var focused = Resolve(motion, "def-dime-bracket", new() { Position = Position.CB, RoleIndex = 0, Task = AssignmentTask.Zone }, 95, 50);

        Assert.True(strongZone.Effect!.Yards > 0);
        Assert.True(weakZone.Effect!.Yards < 0);
        Assert.True(focused.Effect!.Yards <= 0);
        Assert.Contains("zone matchup", strongZone.Effect.Note);
        Assert.Contains("defense answered", weakZone.Effect.Note);
    }

    [Fact]
    public void PassingReadRecordsTheMotionMatchupUsedForTheChosenReceiver()
    {
        var session = GameDayFixture.Session(GameDayMode.Play);
        var play = PlaybookCatalog.OffensivePlays.Single(item => item.Id == "off-empty-motion-cross");
        var offense = session.HomeRoster.Where(player => player.Position == Position.QB).Take(1)
            .Concat(session.HomeRoster.Where(player => player.Position == Position.WR).Take(2))
            .ToArray();
        var defense = session.AwayRoster
            .Where(player => player.Position is Position.CB or Position.FS or Position.SS or Position.OLB or Position.MLB)
            .Take(8)
            .ToArray();
        var read = PassingReadService.Read(
            play,
            PlaybookCatalog.DefensivePlays.Single(item => item.Id == "def-43-cover3"),
            offense,
            defense,
            offense.First(player => player.Position == Position.QB),
            75,
            new Random(41),
            play.PreSnapAction);

        Assert.NotNull(read.PreSnapMatchup);
        Assert.Equal(PreSnapCoverageReaction.ZonePassOff, read.PreSnapMatchup!.Reaction);
        Assert.True(read.PreSnapMatchup.MatchupChanged);
        Assert.NotNull(read.Target);
        Assert.NotNull(read.Defender);
    }

    private static PreSnapMatchup Resolve(
        PreSnapChoice choice,
        string defenseId,
        PlayAssignment defenderAssignment,
        int receiverAbility,
        int defenderAbility)
    {
        var defense = PlaybookCatalog.DefensivePlays.Single(item => item.Id == defenseId);
        return PreSnapMatchupService.Resolve(
            choice,
            defense,
            Position.WR,
            0,
            defenderAssignment,
            true,
            true,
            receiverAbility,
            defenderAbility)!;
    }
}
