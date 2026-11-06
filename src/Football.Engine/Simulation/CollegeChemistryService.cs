using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// Builds the College season culture record from recorded dynasty state: prior-season continuity, roster turnover,
/// coaching and plan changes, morale and discipline.
/// </summary>
public static class CollegeChemistryService
{
    public static DynastyState Refresh(DynastyState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.LeagueTeams.Count == 0)
            return state;

        var completedSeason = state.SeasonYear - 1;
        var profiles = DynastyRosterSimulationProfileBuilder.Build(state, state.LeagueTeams);
        var prior = state.TeamChemistry
            .Where(record => record.SeasonYear == completedSeason)
            .ToDictionary(record => record.TeamName, StringComparer.OrdinalIgnoreCase);

        var rosters = RosterByTeam(state);
        var inputs = new List<TeamChemistryInputs>();
        foreach (var team in state.LeagueTeams)
        {
            if (!profiles.TryGetValue(team.Name, out var profile))
                continue;

            var roster = rosters.GetValueOrDefault(team.Name) ?? Array.Empty<DynastyPlayer>();
            inputs.Add(BuildInputs(state, team.Name, profile, roster, completedSeason));
        }

        var records = inputs
            .Select(input => TeamChemistrySeasonService.Advance(prior.GetValueOrDefault(input.TeamName), input))
            .ToArray();

        var history = state.TeamChemistry
            .Where(record => record.SeasonYear != state.SeasonYear)
            .Concat(records)
            .OrderBy(record => record.SeasonYear)
            .ThenBy(record => record.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var signals = Signals(state, records, prior, rosters, completedSeason);
        var events = LockerRoomEventService.Build(signals);
        var motivation = state.LeagueTeams.Select(team =>
            {
                var priorMotivation = state.TeamMotivation
                    .Where(item => item.TeamName.Equals(team.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(item => item.SeasonYear)
                    .FirstOrDefault();
                return TeamMotivationService.Advance(priorMotivation, signals, team.Name, state.SeasonYear);
            })
            .ToArray();

        return state with
        {
            TeamChemistry = history,
            TeamMotivation = state.TeamMotivation
                .Where(item => item.SeasonYear != state.SeasonYear)
                .Concat(motivation)
                .OrderBy(item => item.SeasonYear)
                .ThenBy(item => item.TeamName, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            LockerRoomEvents = state.LockerRoomEvents
                .Where(item => item.SeasonYear != state.SeasonYear)
                .Concat(events)
                .OrderBy(item => item.SeasonYear)
                .ThenBy(item => item.Week)
                .ThenBy(item => item.TeamName, StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
    }

    public static TeamChemistryRecord? Record(DynastyState state, string teamName, int seasonYear) =>
        state.TeamChemistry.FirstOrDefault(record =>
            record.SeasonYear == seasonYear &&
            record.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase));

    private static TeamChemistryInputs BuildInputs(
        DynastyState state,
        string teamName,
        TeamSimulationProfile profile,
        IReadOnlyList<DynastyPlayer> roster,
        int completedSeason)
    {
        var departures = state.PlayerDepartureHistory
            .Count(departure =>
                departure.SeasonYear == completedSeason &&
                departure.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        var portalExits = state.TransferPortalExitHistory
            .Count(exit =>
                exit.ExitSeasonYear == completedSeason &&
                exit.OriginTeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        var lost = departures + portalExits;
        var baseline = Math.Max(1, roster.Count + lost);
        var turnover = (int)Math.Round(lost * 100.0 / baseline);

        var headCoach = StaffManagementService.GetStaff(state, teamName, StaffRole.HeadCoach);
        var plan = GamePlanService.GetPlan(state, profile);
        var captains = TeamLeadershipService.Captains(roster);
        var names = captains
            .Select(id => roster.FirstOrDefault(player => player.PlayerId == id)?.FullName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToArray();

        var discipline = state.DisciplineHistory
            .Where(incident =>
                incident.SeasonYear == completedSeason &&
                incident.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var disciplineSignal = Math.Clamp(-discipline.Sum(incident => incident.Severity) * 5, -100, 100);
        var moraleSignal = Math.Clamp((GameMoraleService.SeasonMorale(state, teamName, completedSeason) - 60) * 2, -100, 100);

        return new TeamChemistryInputs
        {
            TeamName = teamName,
            SeasonYear = state.SeasonYear,
            EvaluatedCohesion = profile.Cohesion,
            EvaluatedSchemeFamiliarity = profile.SchemeFamiliarity,
            RosterTurnoverPercent = turnover,
            HeadCoachKey = headCoach is null ? "" : headCoach.StaffId.ToString("N"),
            PlanKey = $"{plan.Offense}/{plan.Defense}",
            MoraleSignal = moraleSignal,
            DisciplineSignal = disciplineSignal,
            CaptainIds = captains.ToArray(),
            CaptainNames = names
        };
    }

    private static IReadOnlyList<LockerRoomSignal> Signals(
        DynastyState state,
        IReadOnlyList<TeamChemistryRecord> records,
        IReadOnlyDictionary<string, TeamChemistryRecord> prior,
        IReadOnlyDictionary<string, DynastyPlayer[]> rosters,
        int completedSeason)
    {
        var signals = new List<LockerRoomSignal>();
        foreach (var record in records)
        {
            prior.TryGetValue(record.TeamName, out var previous);
            var roster = rosters.GetValueOrDefault(record.TeamName) ?? Array.Empty<DynastyPlayer>();

            if (previous is not null)
            {
                var headCoach = StaffManagementService.GetStaff(state, record.TeamName, StaffRole.HeadCoach);
                if (headCoach is not null &&
                    !string.IsNullOrEmpty(previous.HeadCoachKey) &&
                    !previous.HeadCoachKey.Equals(headCoach.StaffId.ToString("N"), StringComparison.Ordinal))
                {
                    signals.Add(new LockerRoomSignal
                    {
                        TeamName = record.TeamName,
                        Kind = LockerRoomEventKind.CoachingChange,
                        Subject = headCoach.FullName,
                        SubjectId = headCoach.StaffId,
                        SeasonYear = state.SeasonYear,
                        Week = 0
                    });
                }

                foreach (var captainId in previous.Captains.Where(id => !record.Captains.Contains(id)))
                {
                    var name = roster.FirstOrDefault(player => player.PlayerId == captainId)?.FullName
                        ?? state.PlayerDepartureHistory.FirstOrDefault(departure => departure.PlayerId == captainId)?.FullName
                        ?? state.ActiveRoster.FirstOrDefault(player => player.PlayerId == captainId)?.FullName;
                    signals.Add(new LockerRoomSignal
                    {
                        TeamName = record.TeamName,
                        Kind = LockerRoomEventKind.CaptainChange,
                        Subject = name,
                        SubjectId = captainId,
                        SeasonYear = state.SeasonYear,
                        Week = 0
                    });
                }
            }

            if (record.RosterTurnoverPercent > 0)
            {
                signals.Add(new LockerRoomSignal
                {
                    TeamName = record.TeamName,
                    Kind = LockerRoomEventKind.RosterTurnover,
                    Value = record.RosterTurnoverPercent,
                    SeasonYear = state.SeasonYear,
                    Week = 0
                });
            }

            foreach (var incident in state.DisciplineHistory.Where(item =>
                         item.SeasonYear == completedSeason &&
                         item.TeamName.Equals(record.TeamName, StringComparison.OrdinalIgnoreCase)))
            {
                signals.Add(new LockerRoomSignal
                {
                    TeamName = record.TeamName,
                    Kind = LockerRoomEventKind.Discipline,
                    Subject = incident.PlayerId is null
                        ? null
                        : state.ActiveRoster.FirstOrDefault(player => player.PlayerId == incident.PlayerId)?.FullName,
                    SubjectId = incident.PlayerId,
                    Detail = incident.Description,
                    SeasonYear = state.SeasonYear,
                    Week = incident.Week,
                    Value = incident.Severity
                });
            }

            signals.Add(new LockerRoomSignal
            {
                TeamName = record.TeamName,
                Kind = LockerRoomEventKind.Morale,
                Value = Math.Clamp((GameMoraleService.SeasonMorale(state, record.TeamName, completedSeason) - 60) * 2, -100, 100),
                SeasonYear = state.SeasonYear,
                Week = 0
            });
            signals.Add(new LockerRoomSignal
            {
                TeamName = record.TeamName,
                Kind = LockerRoomEventKind.Chemistry,
                Value = record.Cohesion,
                SeasonYear = state.SeasonYear,
                Week = 0
            });
        }

        return signals;
    }
    private static IReadOnlyDictionary<string, DynastyPlayer[]> RosterByTeam(DynastyState state) =>
        DepthChartService.ProjectRoster(state.ActiveRoster, state.DepthChartAssignments)
            .Where(player => !player.IsRedshirted && player.CurrentInjury is null)
            .GroupBy(player => player.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray(),
                StringComparer.OrdinalIgnoreCase);
}