using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;

namespace ProFootball.Core;

/// <summary>
/// Builds the Pro coach hall of fame from the user's recorded career and CPU club coach season histories.
/// Pro eligibility remains separate from College: a coach becomes eligible once the coaching job is over.
/// </summary>
public static class ProCoachHallOfFameService
{
    public static ProFranchise Refresh(ProFranchise state)
    {
        var entries = CoachHallOfFameService.Build(Candidates(state), state.Year);
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
                .ToList()
        };
    }

    public static IReadOnlyList<CoachHallOfFameCandidate> Candidates(ProFranchise state)
    {
        var candidates = new Dictionary<Guid, CoachHallOfFameCandidate>();
        Guid? nullableUserId = null;
        var career = state.Career;
        if (career is not null)
            nullableUserId = ProCoachHistoryService.StableId(state.Id, career.Name);

        if (career is not null && nullableUserId is { } userId && state.CoachingHistory.Count > 0)
        {
            var playoffYears = PlayoffYears(state, state.UserTeam);
            var titleYears = TitleYears(state, state.UserTeam);
            var seasons = state.CoachingHistory
                .OrderBy(season => season.Year)
                .Select(season => new CoachSeasonRecord
                {
                    StaffId = userId,
                    CoachName = career.Name,
                    SeasonYear = season.Year,
                    TeamName = season.Team,
                    Role = season.Role,
                    Wins = season.Wins,
                    Losses = season.Losses,
                    PlayoffParticipant = playoffYears.Contains(season.Year),
                    NationalChampion = titleYears.Contains(season.Year)
                })
                .ToArray();

            candidates[userId] = new CoachHallOfFameCandidate
            {
                CoachId = userId,
                CoachName = career.Name,
                Seasons = seasons,
                CareerComplete = career.Unemployed,
                RetiredYear = career.Unemployed && seasons.Length > 0 ? seasons[^1].SeasonYear : null
            };
        }

        var currentCoachIds = state.Coaches
            .Where(coach => !coach.Vacant)
            .Select(coach => ProCoachHistoryService.Identity(state, coach))
            .ToHashSet();
        var retirementYears = state.StaffRetirements
            .GroupBy(retirement => ProCoachHistoryService.Identity(state, retirement))
            .ToDictionary(group => group.Key, group => group.Max(retirement => retirement.Year));

        foreach (var group in state.CoachSeasonHistory
                     .Where(record => !nullableUserId.HasValue || record.StaffId != nullableUserId.Value)
                     .GroupBy(record => record.StaffId))
        {
            var seasons = group.OrderBy(record => record.SeasonYear).ToArray();
            retirementYears.TryGetValue(group.Key, out var retiredYear);
            candidates[group.Key] = new CoachHallOfFameCandidate
            {
                CoachId = group.Key,
                CoachName = seasons[^1].CoachName,
                Seasons = seasons,
                CareerComplete = retiredYear > 0 || !currentCoachIds.Contains(group.Key),
                RetiredYear = retiredYear > 0 ? retiredYear : null
            };
        }

        return candidates.Values.ToArray();
    }

    private static HashSet<int> PlayoffYears(ProFranchise state, string teamName) => state.History
        .SelectMany(archive => archive.Games)
        .Concat(state.Games)
        .Where(game => game.Phase == ProPhase.Playoffs && game.Football.InvolvesTeam(teamName))
        .Select(game => game.Football.SeasonYear)
        .ToHashSet();

    private static HashSet<int> TitleYears(ProFranchise state, string teamName) => state.History
        .Where(archive => archive.Champion.Equals(teamName, StringComparison.OrdinalIgnoreCase))
        .Select(archive => archive.Year)
        .ToHashSet();
}
