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
    public void PresnapChoiceIsRecordedAndAppliesItsBoundedEffect()
    {
        var plain = GameDayFixture.Session(GameDayMode.Play);
        var presnap = GameDayFixture.Session(GameDayMode.Play);
        var choice = PreSnapCatalog.Motion("Right", 2);
        var plainSnap = GameDayEngine.Step(plain, "off-spread-zone");
        var alignedSnap = GameDayEngine.Step(presnap, "off-spread-zone", userDefensivePlayId: null, preSnap: choice);

        Assert.Null(plainSnap.PreSnap);
        Assert.Equal(choice, alignedSnap.PreSnap);

        // The call, the outcome and the defensive read are untouched; only the recorded gain moves.
        Assert.Equal(plainSnap.Outcome, alignedSnap.Outcome);
        Assert.Equal(plainSnap.OffensivePlayId, alignedSnap.OffensivePlayId);
        Assert.Equal(plainSnap.DefensivePlayId, alignedSnap.DefensivePlayId);
        Assert.Equal(plainSnap.CallSource, alignedSnap.CallSource);
        Assert.Equal(plainSnap.DefensiveCallSource, alignedSnap.DefensiveCallSource);
        Assert.True(Math.Abs(plainSnap.ClockSeconds - alignedSnap.ClockSeconds) <= 1);

        var effect = PreSnapCatalog.Effect(choice, alignedSnap.Outcome == SnapOutcome.Completion);
        Assert.NotNull(effect);
        if (plainSnap.Yards > 0)
        {
            Assert.Equal(effect!.Yards, alignedSnap.PreSnapEffectYards);
            Assert.Equal(effect.Note, alignedSnap.PreSnapEffectNote);
            Assert.Equal(plainSnap.Yards + effect.Yards, alignedSnap.Yards);
        }
        else
        {
            Assert.Equal(0, alignedSnap.PreSnapEffectYards);
            Assert.Null(alignedSnap.PreSnapEffectNote);
            Assert.Equal(plainSnap.Yards, alignedSnap.Yards);
        }
    }

    [Fact]
    public void PresnapWithoutAnAlignmentActionHasNoEffect()
    {
        var plain = GameDayFixture.Session(GameDayMode.Play);
        var presnap = GameDayFixture.Session(GameDayMode.Play);
        var plainSnap = GameDayEngine.Step(plain, "off-spread-zone");
        var alignedSnap = GameDayEngine.Step(presnap, "off-spread-zone", userDefensivePlayId: null, preSnap: PreSnapCatalog.None);

        Assert.Equal(0, alignedSnap.PreSnapEffectYards);
        Assert.Equal(plainSnap.Yards, alignedSnap.Yards);
        Assert.Equal(plainSnap.Outcome, alignedSnap.Outcome);
    }

    [Fact]
    public void LeagueRulesDecideHowFarAFormationMayMove()
    {
        var tight = FootballRules.College with { MaximumMotionYards = 3, MaximumShiftYards = 1 };
        var wide = FootballRules.College with { MaximumMotionYards = 5 };

        Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 3), tight));
        Assert.False(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 4), tight));
        Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 4), wide));
        Assert.False(PreSnapCatalog.IsValid(new(PreSnapType.Shift, "Left", 2), tight));
    }

    [Fact]
    public void EngineRejectsAPreSnapChoiceThatBreaksLeagueRules()
    {
        var session = GameDayFixture.Session(GameDayMode.Play,
            rules: FootballRules.College with { MaximumMotionYards = 1 });

        Assert.Throws<ArgumentException>(() =>
            GameDayEngine.Step(session, "off-spread-zone", null, PreSnapCatalog.Motion("Left", 2)));
    }


    [Fact]
    public void PlaybookExposesMotionPresetCallsAndTheyRemainValid()
    {
        var motionPlays = PlaybookCatalog.OffensivePlays
            .Where(play => play.PreSnapAction?.Type == PreSnapType.Motion).ToArray();
        Assert.NotEmpty(motionPlays);
        Assert.All(motionPlays, play => Assert.True(PreSnapCatalog.IsValid(play.PreSnapAction!)));
        Assert.All(Enum.GetValues<OffensiveFormation>(),
            formation => Assert.Contains(motionPlays, play => play.Formation == formation));

        var spread = new TeamGamePlan
        {
            TeamName = "Home", Offense = OffensivePlaybook.Spread,
            OffensiveFormation = OffensiveFormation.ShotgunSpread
        };
        Assert.Contains(GamePlanService.AvailableOffensivePlays(spread),
            play => play.PreSnapAction?.Type == PreSnapType.Motion);
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
