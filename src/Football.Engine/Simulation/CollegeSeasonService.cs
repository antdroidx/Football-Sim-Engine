using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Runs the shared season services that write the College's per-season records.</summary>
public static class CollegeSeasonService
{
    public static DynastyState Refresh(DynastyState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state = CollegeHallOfFameService.Refresh(CollegeChemistryService.Refresh(state));
        return state with { Rivalries = RivalryRecordService.Derive(state.Schedule) };
    }
}