using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>League-neutral coach-career HOF scoring. College and Pro hosts decide what history to map.</summary>
public static class CoachHallOfFameService
{
    public static IReadOnlyList<CoachHallOfFameEntry> Build(
        IReadOnlyList<CoachCareerSummary> summaries, int inductionYear)
    {
        ArgumentNullException.ThrowIfNull(summaries);
        var entries = new List<CoachHallOfFameEntry>();
        foreach (var summary in summaries)
        {
            if (summary.HeadCoachSeasons < 5 || summary.HeadCoachWinPercentage < .55) continue;
            var honors = summary.ConferenceTitles * 30 + summary.PlayoffAppearances * 8 +
                summary.NationalTitles * 80;
            if (honors == 0) continue;
            var score = summary.HeadCoachWins * 3 + (int)Math.Round(summary.HeadCoachWinPercentage * 100) + honors;
            entries.Add(new()
            {
                CoachId = summary.StaffId,
                CoachName = summary.CoachName,
                TeamName = summary.Seasons.LastOrDefault()?.TeamName ?? summary.CoachName,
                InductionYear = inductionYear,
                HeadCoachSeasons = summary.HeadCoachSeasons,
                HeadCoachWins = summary.HeadCoachWins,
                HeadCoachLosses = summary.HeadCoachLosses,
                ConferenceTitles = summary.ConferenceTitles,
                PlayoffAppearances = summary.PlayoffAppearances,
                NationalTitles = summary.NationalTitles,
                CareerScore = score,
                HonorsScore = honors,
                Citation = $"{summary.HeadCoachSeasons} HC seasons · {summary.HeadCoachWins}-{summary.HeadCoachLosses} · {summary.ConferenceTitles} conf titles · {summary.PlayoffAppearances} playoff appearances · {summary.NationalTitles} national titles"
            });
        }
        return entries.OrderByDescending(entry => entry.HonorsScore).ThenByDescending(entry => entry.CareerScore)
            .ThenBy(entry => entry.CoachName, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
