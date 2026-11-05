using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Football.Testing;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class ReplayHighlightTests
{
    [Fact]
    public void ScoresTurnoversBigPlaysAndConversionsBecomeCandidates()
    {
        var candidates = ReplayHighlightService.Candidates(new[]
        {
            Snap(1, SnapOutcome.Touchdown, 40),
            Snap(2, SnapOutcome.Interception, 0),
            Snap(3, SnapOutcome.Run, 25),
            Snap(4, SnapOutcome.Run, 12, down: 4, distance: 10)
        });

        Assert.Equal(4, candidates.Count);
        Assert.Equal(ReplayHighlightKind.Score, candidates[0].Kind);
        Assert.Equal(ReplayHighlightKind.Turnover, candidates[1].Kind);
        Assert.Equal(ReplayHighlightKind.BigPlay, candidates[2].Kind);
        Assert.Equal(ReplayHighlightKind.Conversion, candidates[3].Kind);
        Assert.Equal(1, candidates[0].Sequence);
        Assert.Equal(4, candidates[3].Sequence);
        Assert.Contains("Touchdown", candidates[0].Headline);
        Assert.Contains("4th down conversion", candidates[3].Headline);
    }

    [Fact]
    public void OrdinaryPlaysAreNotHighlights()
    {
        var candidates = ReplayHighlightService.Candidates(new[]
        {
            Snap(1, SnapOutcome.Run, 3),
            Snap(2, SnapOutcome.Incompletion, 0)
        });

        Assert.Empty(candidates);
    }

    [Fact]
    public void CandidatesReferenceTheRecordedSnapsWithoutResimulating()
    {
        var session = GameDayFixture.Session(GameDayMode.Play);
        GameDayEngine.Step(session, "off-spread-zone");
        var recorded = session.Events.Count;

        var candidates = ReplayHighlightService.Candidates(session.Events);

        Assert.Equal(recorded, session.Events.Count);
        Assert.All(candidates, candidate => Assert.Contains(candidate.Snap, session.Events));
        Assert.All(candidates, candidate => Assert.Equal(candidate.Snap.Sequence, candidate.Sequence));
    }

    [Fact]
    public void CandidateIdsAreStableForTheSameRecordedSnap()
    {
        var events = new[] { Snap(1, SnapOutcome.Touchdown, 40) };

        var first = ReplayHighlightService.Candidates(events);
        var second = ReplayHighlightService.Candidates(events);

        Assert.Equal(first[0].Id, second[0].Id);
        Assert.Equal("highlight/1", first[0].Id);
    }

    static GameSnapEvent Snap(int sequence, SnapOutcome outcome, int yards, int down = 1, int distance = 10) => new()
    {
        Sequence = sequence, Quarter = 2, ClockSeconds = 600,
        OffenseTeamName = "Home", DefenseTeamName = "Away",
        OffensivePlayId = "off-spread-zone", DefensivePlayId = "def-base",
        Down = down, Distance = distance, StartYardLine = 70,
        BallEndYardLine = Math.Clamp(70 + yards, 0, 100), Yards = yards, Outcome = outcome,
        Points = outcome == SnapOutcome.Touchdown ? 6 : 0
    };
}