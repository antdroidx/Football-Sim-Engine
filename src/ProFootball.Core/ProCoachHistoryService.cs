using DynastyManager.Core.Models;

namespace ProFootball.Core;

/// <summary>Records CPU club coach seasons using stable coach identity across clubs and seasons.</summary>
public static class ProCoachHistoryService
{
    public static ProFranchise RecordCompletedSeason(ProFranchise state)
    {
        if (state.CoachHistoryLastRecordedSeasonYear >= state.Year) return state;

        var rows = ProStandings.Build(state).ToDictionary(row => row.Team.Id);
        var history = state.CoachSeasonHistory.ToList();
        var keys = history.Select(record => (record.StaffId, record.SeasonYear, record.TeamName, record.Role))
            .ToHashSet();
        var playoffTeams = state.Games
            .Where(game => game.Phase == ProPhase.Playoffs && game.Football.HasPlayed)
            .SelectMany(game => new[] { game.Football.HomeTeamName, game.Football.AwayTeamName })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var champions = state.History.Where(archive => archive.Year == state.Year)
            .Select(archive => archive.Champion)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var coach in state.Coaches.Where(coach => !coach.Vacant))
        {
            if (!rows.TryGetValue(coach.TeamId, out var row) || row.Wins + row.Losses + row.Ties == 0) continue;
            var id = coach.StaffId ?? CoachId(state.Id, coach.TeamId, coach.Role, coach.Name);
            if (!keys.Add((id, state.Year, row.Team.Name, coach.Role))) continue;

            var conferenceGames = state.Games.Where(game =>
                game.Phase == ProPhase.RegularSeason &&
                game.Football.HasPlayed &&
                game.Football.InvolvesTeam(row.Team.Name) &&
                Conference(game.Football, row.Team.Name, state)).Select(game => game.Football).ToArray();
            var conferenceWins = conferenceGames.Count(game =>
                game.WinnerTeamName?.Equals(row.Team.Name, StringComparison.OrdinalIgnoreCase) == true);
            var conferenceLosses = conferenceGames.Count(game =>
                game.WinnerTeamName is not null &&
                !game.WinnerTeamName.Equals(row.Team.Name, StringComparison.OrdinalIgnoreCase));
            history.Add(new CoachSeasonRecord
            {
                StaffId = id,
                CoachName = coach.Name,
                SeasonYear = state.Year,
                TeamName = row.Team.Name,
                Role = coach.Role,
                Wins = row.Wins,
                Losses = row.Losses,
                Ties = row.Ties,
                ConferenceWins = conferenceWins,
                ConferenceLosses = conferenceLosses,
                PlayoffParticipant = playoffTeams.Contains(row.Team.Name),
                NationalChampion = champions.Contains(row.Team.Name)
            });
        }

        return state with
        {
            CoachSeasonHistory = history,
            CoachHistoryLastRecordedSeasonYear = state.Year
        };
    }

    /// <summary>Legacy identity for user career records and saves created before coach staff ids existed.</summary>
    public static Guid StableId(Guid franchiseId, string coachName) =>
        HashedId($"{franchiseId:N}:coach:{coachName.Trim().ToUpperInvariant()}");

    /// <summary>Distinct CPU staff identity that does not collide across clubs, roles, or same-name coaches.</summary>
    public static Guid CoachId(Guid franchiseId, string teamId, StaffRole role, string coachName) =>
        HashedId($"{franchiseId:N}:club-coach:{teamId}:{role}:{coachName.Trim().ToUpperInvariant()}");

    public static Guid Identity(ProFranchise state, ProCoach coach) =>
        coach.StaffId ?? CoachId(state.Id, coach.TeamId, coach.Role, coach.Name);

    public static Guid Identity(ProFranchise state, ProStaffRetirement retirement)
    {
        if (retirement.StaffId.HasValue && retirement.StaffId.Value != Guid.Empty) return retirement.StaffId.Value;
        var recorded = state.CoachSeasonHistory
            .Where(record =>
                record.CoachName.Equals(retirement.Name, StringComparison.OrdinalIgnoreCase) &&
                record.TeamName.Equals(retirement.Team, StringComparison.OrdinalIgnoreCase) &&
                record.Role == retirement.Role)
            .Select(record => record.StaffId)
            .FirstOrDefault(id => id != Guid.Empty);
        if (recorded != Guid.Empty) return recorded;
        var teamId = state.League.Teams
            .FirstOrDefault(team => team.Name.Equals(retirement.Team, StringComparison.OrdinalIgnoreCase))?.Id ??
            retirement.Team;
        return CoachId(state.Id, teamId, retirement.Role, retirement.Name);
    }

    private static Guid HashedId(string source)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(source));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static bool Conference(ScheduledGame game, string teamName, ProFranchise state)
    {
        var opponent = game.HomeTeamName == teamName ? game.AwayTeamName : game.HomeTeamName;
        var team = state.League.Teams.FirstOrDefault(item => item.Name == teamName);
        var other = state.League.Teams.FirstOrDefault(item => item.Name == opponent);
        return team is not null && other is not null &&
               team.Conference.Equals(other.Conference, StringComparison.OrdinalIgnoreCase);
    }
}
