using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class PreSnapTests
{
    [Fact]
    public void PreSnapChoicesValidateAndSupportShiftOrMotionOnly()
    {
        var choice = PreSnapCatalog.Motion("Left", 1.5);

        Assert.Equal(PreSnapType.Motion, choice.Type);
        Assert.Equal("Left", choice.Side);
        Assert.Equal(1.5, choice.Yards);
        Assert.True(choice.Valid);
        Assert.False(PreSnapCatalog.IsValid(choice with { Type = (PreSnapType)3 }));
        Assert.False(PreSnapCatalog.IsValid(choice with { Yards = 4.5 }));
        Assert.Equal(5, PreSnapCatalog.Common.Count);
    }

    [Fact]
    public void PresnapChoiceIsRecordedOnScrimmageWithoutChangingTheDeterministicResult()
    {
        var plain = GameDayFixture.Session(GameDayMode.Play);
        var presnap = GameDayFixture.Session(GameDayMode.Play);
        var choice = PreSnapCatalog.Motion("Right", 2);
        var plainSnap = GameDayEngine.Step(plain, "off-spread-zone");
        var alignedSnap = GameDayEngine.Step(presnap, "off-spread-zone", userDefensivePlayId: null, preSnap: choice);

        Assert.Null(plainSnap.PreSnap);
        Assert.Equal(choice, alignedSnap.PreSnap);
        var baseline = (plainSnap.Outcome, plainSnap.Yards, plainSnap.OffensivePlayId, plainSnap.DefensivePlayId,
            plainSnap.ClockSeconds, plainSnap.PreSnapSeconds, plainSnap.LiveBallSeconds,
            plainSnap.CallSource, plainSnap.DefensiveCallSource);
        var aligned = (alignedSnap.Outcome, alignedSnap.Yards, alignedSnap.OffensivePlayId, alignedSnap.DefensivePlayId,
            alignedSnap.ClockSeconds, alignedSnap.PreSnapSeconds, alignedSnap.LiveBallSeconds,
            alignedSnap.CallSource, alignedSnap.DefensiveCallSource);
        Assert.Equal(baseline, aligned);
    }

    [Fact]
    public void PresnapChoicesRejectIllegalSituations()
    {
        var kickoff = GameDayFixture.Session(GameDayMode.Play, includeOpeningKickoff: true);
        var tryPending = GameDayFixture.Session(GameDayMode.Play);
        var special = GameDayFixture.Session(GameDayMode.Play);
        tryPending.TryPending = true;

        Assert.Throws<ArgumentException>(() => GameDayEngine.Step(kickoff, null, null, PreSnapCatalog.Motion("Left", 2)));
        Assert.Throws<ArgumentException>(() => GameDayEngine.Step(tryPending, "special-extra-point", null, PreSnapCatalog.Motion("Left", 2)));
        Assert.Throws<ArgumentException>(() => GameDayEngine.Step(special, "special-kneel", null, PreSnapCatalog.Motion("Left", 2)));
        Assert.Throws<ArgumentException>(() => GameDayEngine.Step(special, "off-spread-zone", null, new((PreSnapType)3, "Left", 2)));
    }

    [Fact]
    public void ControlLayerPassesOnlyLegalOffensivePresnapChoices()
    {
        var session = GameDayFixture.Session(GameDayMode.Play);
        session.PossessionTeamName = session.Game.HomeTeamName;
        session.ControlledTeamName = session.PossessionTeamName;
        session.KickoffPending = false;
        var control = new GameDayLiveControl(session);

        var snap = control.Step("off-spread-zone", null, PreSnapCatalog.Motion("Right", 1.5));
        Assert.Equal(PreSnapCatalog.Motion("Right", 1.5), snap.PreSnap);

        session = GameDayFixture.Session(GameDayMode.Play);
        session.ControlledTeamName = session.Game.HomeTeamName;
        session.DelegateOffense = true;
        control = new(session);
        snap = control.Step("off-spread-zone", null, PreSnapCatalog.Motion("Right", 1.5));
        Assert.Null(snap.PreSnap);
    }
}
