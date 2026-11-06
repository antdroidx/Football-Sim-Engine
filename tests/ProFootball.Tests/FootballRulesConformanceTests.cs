using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public sealed class FootballRulesConformanceTests
{
    static ProFranchise State() => ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
    static GameDaySession Session(ProFranchise state, FootballRules? rules = null)
    {
        var game = state.Games[0].Football;
        return GameDayEngine.Start(game,
            ProLeagueService.Profile(state, game.HomeTeamName),
            ProLeagueService.Profile(state, game.AwayTeamName),
            state.Players.Where(p => !p.Retired && !p.Reserve && !p.PracticeSquad)
                .Select(p => p.Football).ToArray(), GameDayMode.Simulate,
            state.DepthChartAssignments, rules ?? FootballRules.Professional);
    }

    [Fact]
    public void ProfessionalOvertimeUsesLeagueClockAndTimeoutPolicy()
    {
        var state = State();
        var regular = Session(state);
        regular.OvertimeRound = 1; regular.Quarter = 5; regular.ClockSeconds = 0;
        regular.HomeScore = regular.AwayScore; regular.KickoffPending = false;
        GameDayEngine.Step(regular);
        Assert.True(regular.IsComplete);
        Assert.Equal(600, FootballRules.Professional.RegularOvertimeSeconds);
        Assert.Equal(900, FootballRules.Professional.PlayoffOvertimeSeconds);

        var postseason = Session(state, FootballRules.Professional with { Postseason = true });
        postseason.OvertimeRound = 1; postseason.Quarter = 5; postseason.ClockSeconds = 0;
        postseason.HomeScore = postseason.AwayScore; postseason.KickoffPending = false;
        GameDayEngine.Step(postseason);
        Assert.False(postseason.IsComplete);
        Assert.Equal(6, postseason.Quarter);
        Assert.Equal(900, postseason.ClockSeconds);
    }

    [Fact]
    public void OvertimeGivesTheSecondTeamAPossessionAfterAFirstTeamTouchdown()
    {
        var state = State();
        var session = Session(state);
        session.OvertimeRound = 1; session.Quarter = 5; session.ClockSeconds = 600;
        session.KickoffPending = false; session.TryPending = true; session.OvertimePossessions = 0;
        session.PossessionTeamName = session.OvertimeFirstTeam = session.Game.HomeTeamName;
        session.HomeScore = 6; session.AwayScore = 0;
        GameDayEngine.Step(session, "special-extra-point");
        Assert.False(session.IsComplete);
        Assert.Equal(1, session.OvertimePossessions);
        Assert.Equal(session.Game.AwayTeamName, session.PossessionTeamName);
    }

    [Fact]
    public void LeagueRulesKeepFirstDownAndSackAccountingSeparate()
    {
        Assert.True(FootballRules.College.StopClockOnFirstDown);
        Assert.False(FootballRules.Professional.StopClockOnFirstDown);
        Assert.True(FootballRules.College.SacksCountAsRushing);
        Assert.False(FootballRules.Professional.SacksCountAsRushing);

        var state = State();
        var session = Session(state);
        var qb = session.HomeRoster.First(p => p.Position == Position.QB);
        session.Events.Add(new GameSnapEvent
        {
            StatsVersion = 1, Sequence = 1, PossessionNumber = 1, Quarter = 1, ClockSeconds = 700,
            OffenseTeamName = session.Game.HomeTeamName, DefenseTeamName = session.Game.AwayTeamName,
            OffensivePlayId = "off-spread-verts", DefensivePlayId = "", QuarterbackId = qb.PlayerId,
            Outcome = SnapOutcome.Sack, Yards = -7, Down = 1, Distance = 10
        });
        session.IsComplete = true;
        var result = GameDayEngine.Finish(session);
        Assert.Equal(0, result.HomeStats!.RushAttempts);
        Assert.Equal(0, result.HomeStats.PassAttempts);
        Assert.Equal(7, result.HomeStats.SackYardsLost);
    }

    [Fact]
    public void UnsourcedRivalryAndPreSnapLimitsKeepTheSharedPolicyBoundary()
    {
        Assert.Equal(1, FootballRules.College.MaximumShiftYards);
        Assert.Equal(3, FootballRules.College.MaximumMotionYards);
        Assert.Equal(FootballRules.College.MaximumShiftYards, FootballRules.Professional.MaximumShiftYards);
        Assert.Equal(FootballRules.College.MaximumMotionYards, FootballRules.Professional.MaximumMotionYards);
        Assert.All(typeof(RivalryRecord).GetProperties(), property =>
            Assert.DoesNotContain("Effect", property.Name, StringComparison.OrdinalIgnoreCase));
        Assert.True(PreSnapCatalog.IsValid(PreSnapCatalog.Motion("Left", 3), FootballRules.Professional));
    }

    [Fact]
    public void ProPunterAndKickerHaveExplicitCareerPeaks()
    {
        Assert.Equal(32, ProDevelopment.PeakAge(Position.P));
        Assert.Equal(33, ProDevelopment.PeakAge(Position.K));
    }
}
