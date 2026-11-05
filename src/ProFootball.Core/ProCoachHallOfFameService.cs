using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;

namespace ProFootball.Core;

/// <summary>
/// The Pro coach hall of fame is built from the user's recorded coaching career. League rules keep the Pro
/// eligibility separate from College: a Pro coach is considered once the coaching job is over.
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
        if (state.Career is null || state.CoachingHistory.Count == 0)
            return [];

        var coachId = StableId(state.Id, state.Career.Name);
        var playoffYears = PlayoffYears(state);
        var titleYears = TitleYears(state);
        var seasons = state.CoachingHistory
            .OrderBy(season => season.Year)
            .Select(season => new CoachSeasonRecord
            {
                StaffId = coachId,
                CoachName = state.Career!.Name,
                SeasonYear = season.Year,
                TeamName = season.Team,
                Role = season.Role,
                Wins = season.Wins,
                Losses = season.Losses,
                PlayoffParticipant = playoffYears.Contains(season.Year),
                NationalChampion = titleYears.Contains(season.Year)
            })
            .ToArray();

        return
        [
            new CoachHallOfFameCandidate
            {
                CoachId = coachId,
                CoachName = state.Career.Name,
                Seasons = seasons,
                CareerComplete = state.Career.Unemployed,
                RetiredYear = state.Career.Unemployed && seasons.Length > 0 ? seasons[^1].SeasonYear : null
            }
        ];
    }

    private static HashSet<int> PlayoffYears(ProFranchise state) => state.History
        .SelectMany(archive => archive.Games)
        .Concat(state.Games)
        .Where(game => game.Phase == ProPhase.Playoffs && game.Football.InvolvesTeam(state.UserTeam))
        .Select(game => game.Football.SeasonYear)
        .ToHashSet();

    private static HashSet<int> TitleYears(ProFranchise state) => state.History
        .Where(archive => archive.Champion.Equals(state.UserTeam, StringComparison.OrdinalIgnoreCase))
        .Select(archive => archive.Year)
        .ToHashSet();

    private static Guid StableId(Guid franchiseId, string coachName)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{franchiseId}:{coachName}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}