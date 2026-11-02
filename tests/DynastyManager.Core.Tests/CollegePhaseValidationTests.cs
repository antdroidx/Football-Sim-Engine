using DynastyManager.Core.Models;
using DynastyManager.Core.Seasons;
using Xunit;
namespace DynastyManager.Core.Tests;
[Trait("Owner", "College")]
[Trait("Category", "Fast")]
public sealed class CollegePhaseValidationTests
{
    [Fact]
    public void CollegeJobGateDoesNotAlterCalendarStateOrPersistAResult()
    {
        var state = new DynastyState { DynastyId = Guid.NewGuid(), DynastyName = "Phase test", UserTeamName = "A",
            SeasonYear = 2026, Phase = SeasonPhase.Recruiting, Week = 20, UserCoachIsFired = true };
        var blocked = CollegePhaseValidation.Evaluate(state);
        Assert.False(blocked.CanAdvance); Assert.Equal("coaching-job-required", blocked.Blockers[0].Code);
        Assert.True(CollegePhaseValidation.Evaluate(state with { UserCoachIsFired = false }).CanAdvance);
        Assert.Equal(SeasonPhase.Recruiting, state.Phase); Assert.Equal(20, state.Week);
    }
}
