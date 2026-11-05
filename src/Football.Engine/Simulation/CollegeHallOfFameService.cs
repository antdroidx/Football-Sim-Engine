using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// Builds College coach hall of fame candidates from the recorded coach history. A coach becomes eligible once the
/// program no longer lists them on staff, and the induction is recorded once.
/// </summary>
public static class CollegeHallOfFameService
{
    public static DynastyState Refresh(DynastyState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var entries = CoachHallOfFameService.Build(Candidates(state), state.SeasonYear);
        if (entries.Count == 0)
            return state;

        var recorded = state.CoachHallOfFame.ToDictionary(entry => entry.CoachId);
        foreach (var entry in entries)
        {
            // An inductee keeps the year they were inducted.
            recorded[entry.CoachId] = recorded.TryGetValue(entry.CoachId, out var existing)
                ? entry with { InductionYear = existing.InductionYear }
                : entry;
        }

        return state with
        {
            CoachHallOfFame = recorded.Values
                .OrderByDescending(entry => entry.InductionYear)
                .ThenByDescending(entry => entry.HonorsScore)
                .ThenBy(entry => entry.CoachName, StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
    }

    public static IReadOnlyList<CoachHallOfFameCandidate> Candidates(DynastyState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var coaching = state.Staff.Select(staff => staff.StaffId).ToHashSet();
        if (state.UserHeadCoach is not null)
            coaching.Add(state.UserHeadCoach.StaffId);

        var retirements = state.StaffRetirements
            .GroupBy(record => record.Coach.StaffId)
            .ToDictionary(group => group.Key, group => group.Max(record => record.SeasonYear));

        return state.CoachSeasonHistory
            .GroupBy(record => record.StaffId)
            .Select(group =>
            {
                var seasons = group.OrderBy(record => record.SeasonYear).ToArray();
                var retiredYear = retirements.GetValueOrDefault(group.Key);
                return new CoachHallOfFameCandidate
                {
                    CoachId = group.Key,
                    CoachName = seasons.LastOrDefault()?.CoachName ?? "Coach",
                    Seasons = seasons,
                    CareerComplete = retiredYear > 0 || !coaching.Contains(group.Key),
                    RetiredYear = retiredYear > 0 ? retiredYear : null
                };
            })
            .ToArray();
    }
}