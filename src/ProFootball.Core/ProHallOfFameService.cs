using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;

namespace ProFootball.Core;

/// <summary>Derived Pro Hall of Fame eligibility from recorded history and honors. No save schema change.</summary>
public static class ProHallOfFameService
{
    public static IReadOnlyList<HallOfFameEntry> Build(ProFranchise state)
    {
        var lines = state.History.SelectMany(h => h.Games).Concat(state.Games)
            .Where(g => g.Phase == ProPhase.RegularSeason && g.Football.HasPlayed)
            .DistinctBy(g => g.Football.GameId)
            .SelectMany(g => g.Football.PlayerStats)
            .ToArray();
        var retired = state.Players.Where(p => p.Retired).ToArray();
        var entries = new List<HallOfFameEntry>();
        foreach (var player in retired)
        {
            var lineId = player.Football.PlayerId;
            var careerLines = lines.Where(p => p.PlayerId == lineId).ToArray();
            if (careerLines.Length == 0) continue;
            var awards = state.Honors
                .SelectMany(h => h.Awards)
                .Where(a => a.PlayerId == lineId)
                .ToArray();
            var allPro = state.Honors
                .SelectMany(h => h.AllPros)
                .Where(p => p.Player.Football.PlayerId == lineId)
                .ToArray();
            var allStar = state.Honors
                .SelectMany(h => h.Selections)
                .Where(p => p.Player.Football.PlayerId == lineId)
                .ToArray();
            var honorsScore = awards.Sum(a => a.Title.Contains("MVP", StringComparison.OrdinalIgnoreCase) ? 60 :
                    a.Title.Contains("Player of the Year", StringComparison.OrdinalIgnoreCase) ? 40 :
                    a.Title.Contains("Rookie of the Year", StringComparison.OrdinalIgnoreCase) ? 15 : 5) +
                allPro.Sum(p => p.Team == 1 ? 25 : 10) +
                allStar.Length * 5;
            var major = awards.Any(a => a.Title.Contains("MVP", StringComparison.OrdinalIgnoreCase) ||
                a.Title.Contains("Most Valuable Player", StringComparison.OrdinalIgnoreCase) ||
                a.Title.Contains("Player of the Year", StringComparison.OrdinalIgnoreCase));
            if (!major) continue;
            entries.Add(new HallOfFameEntry
            {
                PlayerId = lineId,
                PlayerName = player.Football.FullName,
                TeamName = player.Football.TeamName,
                Position = player.Football.Position,
                InductionYear = state.Year,
                CareerScore = (int)careerLines.Sum(PlayerCareerScoreService.Production),
                HonorsScore = honorsScore,
                RecordedSeasons = careerLines.Select(l => l.SeasonYear).Distinct().Count(),
                Citation = $"{careerLines.Length} recorded games · {honorsScore} honors score · career score {careerLines.Sum(PlayerCareerScoreService.Production):N0}"
            });
        }
        return entries.OrderByDescending(e => e.HonorsScore).ThenByDescending(e => e.CareerScore)
            .ThenBy(e => e.PlayerName, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static IReadOnlyList<CoachCareerSummary> CoachSummaries(ProFranchise state) => state
        .CoachingHistory
        .GroupBy(h => h.Team, StringComparer.OrdinalIgnoreCase)
        .Select(g =>
        {
            var seasons = g.OrderBy(h => h.Year).ToArray();
            var idBytes = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{state.Id}:{g.Key}:{seasons.Length}"));
            var coachId = new Guid(idBytes.AsSpan(0, 16));
            return new CoachCareerSummary
            {
                StaffId = coachId,
                CoachName = state.Career?.Name ?? $"{g.Key} coach",
                Seasons = seasons.Select(h => new CoachSeasonRecord
                {
                    StaffId = coachId,
                    CoachName = state.Career?.Name ?? $"{h.Team} coach",
                    SeasonYear = h.Year,
                    TeamName = h.Team,
                    Role = h.Role,
                    Wins = h.Wins,
                    Losses = h.Losses
                }).ToArray()
            };
        })
        .ToArray();
}
