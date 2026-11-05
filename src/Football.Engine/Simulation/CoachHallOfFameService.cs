using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>A recorded coaching career a host offers for hall of fame induction.</summary>
public sealed record CoachHallOfFameCandidate
{
    public required Guid CoachId { get; init; }
    public required string CoachName { get; init; }
    public required IReadOnlyList<CoachSeasonRecord> Seasons { get; init; }
    /// <summary>True once the coaching career is over and the host may consider induction.</summary>
    public bool CareerComplete { get; init; }
    public int? RetiredYear { get; init; }
}

/// <summary>
/// League-neutral coach-career hall of fame scoring. College and Pro hosts decide which careers are complete,
/// what honors a season carries, and when a coach may be inducted.
/// </summary>
public static class CoachHallOfFameService
{
    public const int MinimumHeadCoachSeasons = 5;
    public const double MinimumWinPercentage = .55;

    public static IReadOnlyList<CoachHallOfFameEntry> Build(IReadOnlyList<CoachHallOfFameCandidate> candidates, int inductionYear)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var entries = new List<CoachHallOfFameEntry>();
        foreach (var candidate in candidates)
        {
            if (!candidate.CareerComplete)
                continue;

            var headCoachSeasons = candidate.Seasons.Count(season => season.Role == StaffRole.HeadCoach);
            if (headCoachSeasons < MinimumHeadCoachSeasons)
                continue;

            var wins = candidate.Seasons.Where(season => season.Role == StaffRole.HeadCoach).Sum(season => season.Wins);
            var losses = candidate.Seasons.Where(season => season.Role == StaffRole.HeadCoach).Sum(season => season.Losses);
            var winPercentage = wins + losses == 0 ? 0 : (double)wins / (wins + losses);
            if (winPercentage < MinimumWinPercentage)
                continue;

            var conferenceTitles = candidate.Seasons.Count(season => season.Role == StaffRole.HeadCoach && season.ConferenceChampion);
            var playoffAppearances = candidate.Seasons.Count(season => season.Role == StaffRole.HeadCoach && season.PlayoffParticipant);
            var nationalTitles = candidate.Seasons.Count(season => season.Role == StaffRole.HeadCoach && season.NationalChampion);
            var honors = conferenceTitles * 30 + playoffAppearances * 8 + nationalTitles * 80;
            if (honors == 0)
                continue;

            var spans = TeamSpans(candidate.Seasons);
            entries.Add(new CoachHallOfFameEntry
            {
                CoachId = candidate.CoachId,
                CoachName = candidate.CoachName,
                TeamName = spans.Count > 0 ? spans[^1].TeamName : candidate.CoachName,
                InductionYear = inductionYear,
                HeadCoachSeasons = headCoachSeasons,
                HeadCoachWins = wins,
                HeadCoachLosses = losses,
                ConferenceTitles = conferenceTitles,
                PlayoffAppearances = playoffAppearances,
                NationalTitles = nationalTitles,
                CareerScore = wins * 3 + (int)Math.Round(winPercentage * 100) + honors,
                HonorsScore = honors,
                Retired = candidate.CareerComplete,
                RetiredYear = candidate.RetiredYear,
                TeamCitations = spans.Select(span => span.Citation).ToArray(),
                Citation = $"{headCoachSeasons} HC seasons · {wins}-{losses} · {conferenceTitles} conf titles · {playoffAppearances} playoff appearances · {nationalTitles} national titles"
            });
        }

        return entries.OrderByDescending(entry => entry.HonorsScore)
            .ThenByDescending(entry => entry.CareerScore)
            .ThenBy(entry => entry.CoachName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>Groups a coach's head coaching seasons into one span per team.</summary>
    public static IReadOnlyList<CoachHallOfFameTeamSpan> TeamSpans(IReadOnlyList<CoachSeasonRecord> seasons)
    {
        ArgumentNullException.ThrowIfNull(seasons);

        return seasons
            .Where(season => season.Role == StaffRole.HeadCoach)
            .GroupBy(season => season.TeamName, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var ordered = group.OrderBy(season => season.SeasonYear).ToArray();
                return new CoachHallOfFameTeamSpan
                {
                    TeamName = ordered[0].TeamName,
                    FirstYear = ordered[0].SeasonYear,
                    LastYear = ordered[^1].SeasonYear,
                    Seasons = ordered.Length,
                    Wins = ordered.Sum(season => season.Wins),
                    Losses = ordered.Sum(season => season.Losses)
                };
            })
            .OrderBy(span => span.FirstYear)
            .ToArray();
    }
}