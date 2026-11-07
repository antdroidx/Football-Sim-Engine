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
    public void InternalSimulationBoundsDecideCatalogDistance()
    {
        var tight = FootballRules.College with { MaximumMotionYards = 3, MaximumShiftYards = 1 };
        var wide = FootballRules.College with { MaximumMotionYards = 5 };

        Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 3), tight));
        Assert.False(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 4), tight));
        Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 4), wide));
        Assert.False(PreSnapCatalog.IsValid(new(PreSnapType.Shift, "Left", 2), tight));
    }

    [Fact]
    public void SourcedRulesBoundMotionCountDirectionSetAndLineStart()
    {
        foreach (var rules in new[] { FootballRules.College, FootballRules.Professional })
        {
            Assert.Equal(1, rules.MaximumBackfieldPlayersInMotion);
            Assert.True(rules.MotionMustBeParallelOrAwayFromGoalLine);
            Assert.True(rules.MotionFromLineRequiresBackAndCompleteStop);
            Assert.True(rules.RequiresOneSecondSetAfterShift);

            Assert.True(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2, PreSnapMotionDirection.AwayFromGoalLine), rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2, PreSnapMotionDirection.TowardGoalLine), rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2) with { BackfieldPlayersInMotion = 2 }, rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2) with { AllOtherPlayersStationary = false }, rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2) with { FollowedShift = true, SetForOneSecond = false }, rules));
            Assert.True(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2) with { SetForOneSecond = false }, rules));
            Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.Jet("Left", Position.WR, 1), rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Jet("Left", Position.WR, 1) with { FollowedShift = true }, rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Shift("Left", .5) with { SetForOneSecond = false }, rules));
            Assert.False(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2) with
                {
                    StartedOnLineOfScrimmage = true,
                    BecameBackAndStopped = false
                }, rules));
            Assert.True(PreSnapCatalog.IsValid(
                PreSnapCatalog.Motion("Left", 2) with
                {
                    StartedOnLineOfScrimmage = true,
                    BecameBackAndStopped = true
                }, rules));
        }
    }

    [Fact]
    public void OlderRecordedPreSnapChoiceDefaultsToASourcedLegalMotion()
    {
        var restored = System.Text.Json.JsonSerializer.Deserialize<PreSnapChoice>(
            """{"Type":2,"Side":"Right","Yards":2}""")!;

        Assert.True(restored.Valid);
        Assert.Equal(PreSnapMotionDirection.Lateral, restored.MotionDirection);
        Assert.Equal(PreSnapMotionKind.ReturnToFormation, restored.MotionKind);
        Assert.Null(restored.MotionPosition);
        Assert.False(restored.MotionLiveAtSnap);
        Assert.Equal(1, restored.BackfieldPlayersInMotion);
        Assert.True(restored.AllOtherPlayersStationary);
        Assert.True(restored.SetForOneSecond);
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
        var spreadBook = GamePlanService.AvailableOffensivePlays(spread);
        Assert.Contains(spreadBook, play => play.Name == "Jet Sweep");
        Assert.Contains(spreadBook, play => play.Name == "Glide Motion Power");
        Assert.Contains(spreadBook, play => play.Name == "Motion Crossers");

        var playAction = new TeamGamePlan
        {
            TeamName = "Home", Offense = OffensivePlaybook.PlayAction,
            OffensiveFormation = OffensiveFormation.Singleback
        };
        var playActionBook = GamePlanService.AvailableOffensivePlays(playAction);
        Assert.Contains(playActionBook, play => play.Name == "Orbit Motion Levels");
        Assert.Contains(playActionBook, play => play.Name == "Orbit Motion PA Boot");
        Assert.Contains(playActionBook, play => play.Name == "Glide Motion Power");
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

    [Fact]
    public void UserOptionsRestrictShiftsToReceiversAndKeepMotionPlayAware()
    {
        var jetPlay = PlaybookCatalog.OffensivePlays.Single(play => play.Id == "off-spread-jet-zone");
        var options = PreSnapCatalog.Options(jetPlay);

        Assert.Equal(4, options.Count);
        Assert.Equal(PreSnapCatalog.None, options[0]);
        Assert.All(options.Where(choice => choice.Type == PreSnapType.Shift),
            choice => Assert.Equal(Position.WR, choice.ShiftPosition));
        Assert.Contains(jetPlay.PreSnapAction, options);
        Assert.DoesNotContain(options, choice =>
            choice.Type == PreSnapType.Shift && choice.ShiftPosition != Position.WR);
        Assert.DoesNotContain(options, choice =>
            choice.Type == PreSnapType.Motion && choice.MotionPosition is not (Position.WR or Position.TE or Position.RB or Position.FB));

        Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.ReceiverShift("Right", Position.WR, 0)));
        Assert.False(PreSnapCatalog.IsValid(PreSnapCatalog.ReceiverShift("Right", Position.OL, 0)));
    }
}
