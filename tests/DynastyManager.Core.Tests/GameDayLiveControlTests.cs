using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public class GameDayLiveControlTests
{
    static GameDayLiveControl Create(FootballRules? rules = null)
    {
        var session = GameDayEngineTests.Session(GameDayMode.Play);
        if (rules is not null) session = GameDayEngine.Start(session.Game, session.HomeProfile, session.AwayProfile, session.Roster, GameDayMode.Play, rules: rules);
        session.PossessionTeamName = session.Game.HomeTeamName;
        session.ControlledTeamName = session.PossessionTeamName;
        session.KickoffPending = false;
        return new(session);
    }
    [Theory]
    [InlineData(true, true)] [InlineData(true, false)] [InlineData(false, true)] [InlineData(false, false)]
    public void UnitOwnershipDeterminesActualAdvancement(bool offense, bool defense)
    {
        var c = Create(); c.SetManual(false, offense); c.SetManual(true, defense);
        Assert.Equal(offense, c.NeedsCall); Assert.Equal(!offense, c.ShouldAdvance);
        c.Session.PossessionTeamName = c.Session.ControlledTeamName == c.Session.Game.HomeTeamName ? c.Session.Game.AwayTeamName : c.Session.Game.HomeTeamName;
        Assert.Equal(defense, c.NeedsCall); Assert.Equal(!defense, c.ShouldAdvance);
    }
    [Fact] public void PauseDuringSnapStopsTheNextSnapAndDoesNotChangeOwnership()
    {
        var c = Create(); c.SetManual(false, false);
        Assert.True(c.TryBegin(false)); c.Pause();
        Assert.Equal("Pausing after this play", c.Status); Assert.False(c.TryBegin(false));
        c.End(); Assert.False(c.ShouldAdvance); Assert.True(c.Session.DelegateOffense);
        c.Resume(); Assert.True(c.ShouldAdvance);
    }
    [Fact] public void ReclaimingUnitStopsBeforeNextDecision()
    {
        var c = Create(); c.Watch(); Assert.True(c.TryBegin(false));
        c.SetManual(false, true); c.End(); Assert.True(c.NeedsCall); Assert.False(c.ShouldAdvance);
        Assert.False(c.Manual(true));
    }
    [Fact] public void NestedMenusHoldWithoutChangingPauseOrOwnership()
    {
        var c = Create(); c.Watch();
        using (c.Suspend()) { using(c.Suspend()) Assert.False(c.ShouldAdvance); Assert.False(c.ShouldAdvance); }
        Assert.True(c.ShouldAdvance); c.Pause();
        using(c.Suspend()) Assert.False(c.ShouldAdvance);
        Assert.True(c.IsPaused); Assert.False(c.ShouldAdvance);
    }
    [Theory] [InlineData(StaffRole.OffensiveCoordinator, true)] [InlineData(StaffRole.DefensiveCoordinator, false)]
    public void TakeoverPreservesCareerRoleLocks(StaffRole role, bool lockedDefense)
    {
        var c = Create(); c.Session.UserCoachRole = role; c.Watch(); c.TakeOver();
        Assert.False(c.Manual(lockedDefense)); Assert.True(c.Manual(!lockedDefense));
        Assert.Throws<InvalidOperationException>(() => c.SetManual(lockedDefense, true));
    }
    [Fact] public void DelegationDiscardsStaleUserPlayOnActualEngineStep()
    {
        var c = Create(); c.SetManual(false, false);
        var snap = c.Step("special-spike", null);
        Assert.NotEqual(SnapOutcome.Spike, snap.Outcome);
        Assert.NotEqual(PlayCallSource.User, snap.CallSource);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void ConversionsRespectLeagueOvertimeRules(bool professional)
    {
        var c = Create(professional ? FootballRules.Professional : FootballRules.College); c.Session.TryPending = true;
        Assert.Contains("special-extra-point", c.SpecialCalls());
        c.Session.OvertimeRound = 2;
        Assert.Equal(professional, c.SpecialCalls().Contains("special-extra-point"));
        Assert.Contains("special-two-point", c.SpecialCalls());
    }
    [Fact] public void DelegatedDefenseActuallyUsesCpuCallSource()
    {
        var c = Create(); c.Session.PossessionTeamName = c.Session.Game.AwayTeamName;
        c.SetManual(true, false);
        var snap = c.Step(null, PlaybookCatalog.DefensivePlays.First().Id);
        Assert.NotEqual(PlayCallSource.User, snap.DefensiveCallSource);
    }
    [Fact] public void KickoffsAndOpponentConversionsAdvanceWithoutAnUnusablePicker()
    {
        var c = Create(); c.Session.KickoffPending = true;
        Assert.True(c.ShouldAdvance); Assert.False(c.NeedsCall);
        c.Session.KickoffPending = false; c.Session.TryPending = true; c.Session.PossessionTeamName = c.Session.Game.AwayTeamName;
        Assert.True(c.ShouldAdvance); Assert.False(c.CanCall(true));
    }
    [Fact] public void CompletedGamesCannotStartAnotherSnap()
    {
        var c = Create(); c.Session.IsComplete = true;
        Assert.False(c.TryBegin(true)); Assert.False(c.TryBegin(false));
    }
    [Theory] [InlineData(25)] [InlineData(35)]
    public void KickoffCommentaryUsesTheRecordedLeagueSpot(int yardLine)
    {
        var c = Create();
        var snap = new GameSnapEvent { OffenseTeamName = "Home", DefenseTeamName = "Away", OffensivePlayId = "", DefensivePlayId = "",
            Outcome = SnapOutcome.Kickoff, IsTouchback = true, BallEndYardLine = yardLine };
        Assert.Contains($"{yardLine}-yard line", GameDayCommentaryService.Describe(snap, c.Session.PlayersById));
    }
    [Theory]
    [InlineData(StaffRole.HeadCoach, false, true)]
    [InlineData(StaffRole.HeadCoach, true, true)]
    [InlineData(StaffRole.OffensiveCoordinator, true, false)]
    [InlineData(StaffRole.DefensiveCoordinator, false, false)]
    public void BothCallerEntriesRemainNamedAndLocksExplainTheCareerRole(StaffRole role, bool defending, bool permitted)
    {
        var c = Create(); c.Session.UserCoachRole = role;
        Assert.StartsWith("Offense: ", c.CallerLabel(false));
        Assert.StartsWith("Defense: ", c.CallerLabel(true));
        Assert.Equal(permitted, c.Permitted(defending));
        if (permitted) Assert.Null(c.Restriction(defending));
        else { Assert.Contains("role locked", c.CallerLabel(defending)); Assert.Contains(UserCoachControlService.Label(role), c.Restriction(defending)!); }
    }
    [Fact] public void HeadCoachDefenseSelectionActuallyReachesTheEngine()
    {
        var c = Create(); c.Session.PossessionTeamName = c.Session.Game.AwayTeamName;
        Assert.True(c.CanCall(true));
        var choice = PlaybookCatalog.DefensivePlays.First().Id;
        var snap = c.Step(null, choice);
        Assert.Equal(choice, snap.DefensivePlayId);
        Assert.Equal(PlayCallSource.User, snap.DefensiveCallSource);
    }
    [Fact] public void BusyAndSuspendedStateRejectDuplicateManualSnaps()
    {
        var c = Create(); using(c.Suspend()) Assert.False(c.TryBegin(true));
        Assert.True(c.TryBegin(true)); Assert.False(c.TryBegin(true)); c.End(); Assert.True(c.TryBegin(true));
    }
}
