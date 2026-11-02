using DynastyManager.Core.Models;

namespace DynastyManager.Core.Seasons;

public static class CollegePhaseValidation
{
    public static PhaseValidation Evaluate(DynastyState state) => state.UserCoachIsFired
        ? PhaseValidation.Blocked("coaching-job-required",
            "You were fired from your previous program. Open Program and accept a coaching offer before advancing.", "Program")
        : PhaseValidation.Ready;
}
