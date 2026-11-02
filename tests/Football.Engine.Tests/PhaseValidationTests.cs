using DynastyManager.Core.Models;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner", "Engine")]
[Trait("Category", "Fast")]
public sealed class PhaseValidationTests
{
    [Fact]
    public void ReadyAndBlockedResultsKeepReasonAndActionTogether()
    {
        Assert.True(PhaseValidation.Ready.CanAdvance); Assert.Null(PhaseValidation.Ready.FirstReason);
        var blocked = PhaseValidation.Blocked("limit", "Resolve roster", "Roster");
        Assert.False(blocked.CanAdvance); Assert.Equal("Resolve roster", blocked.FirstReason);
        Assert.Equal("Roster", Assert.Single(blocked.Blockers).ActionKey);
    }
}
