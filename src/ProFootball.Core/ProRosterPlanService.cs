using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

public sealed record RosterPositionDemand(Position Position, int Target, int Starters, int Minimum, string SchemeUnit)
{
    public bool NeedsPlayer => Target > Starters;
}

/// <summary>
/// Uses the shared football identity to rank pro acquisitions by scheme fit, positional priority,
/// depth need, and cap efficiency. Financial/rule validation remains in ProEconomy/ProFinance.
/// </summary>
public static class ProRosterPlanService
{
    public static TeamIdentity GetIdentity(ProFranchise state, string team)
    {
        var club = state.League.Teams.FirstOrDefault(item => item.Name.Equals(team, StringComparison.OrdinalIgnoreCase));
        var plan = state.GamePlans.LastOrDefault(item => item.TeamName.Equals(team, StringComparison.OrdinalIgnoreCase));
        var head = state.Coaches.FirstOrDefault(item => club is not null && item.TeamId == club.Id && item.Role == StaffRole.HeadCoach && !item.Vacant);
        var identity = club is not null && ProTeamStrategyCatalog.TryGet(club.Id, out var strategy) ? strategy : null;
        var offense = plan?.Offense ?? identity?.Offense ?? OffensivePlaybook.Balanced;
        var defense = plan?.Defense ?? identity?.Defense ?? DefensivePlaybook.Balanced;
        var approach = plan?.Approach ?? DefensiveApproach.Balanced;
        (offense, defense) = RosterPlanService.AdaptToPersonnel(
            offense,
            defense,
            RosterPlanService.PersonnelProfile(Active(state, team).Select(item => item.Football).ToArray()));
        var attributes = head is null ? null : ProStaff.Profile(head).Attributes;
        var pressure = approach switch
        {
            DefensiveApproach.Pressure => .82,
            DefensiveApproach.ProtectPass => .48,
            DefensiveApproach.StopRun => .45,
            _ => .62
        };

        return new TeamIdentity
        {
            TeamName = team,
            Offense = offense,
            Defense = defense,
            Approach = approach,
            PressureTendency = (int)Math.Round(pressure * 100),
            ZoneOrProtectTendency = (int)Math.Round((1-pressure) * 100),
            TalentVersusFit = attributes is null ? .55 : Math.Clamp(.50 + (attributes.OffensePlaycalling - 65)*.005, .35, .75),
            DevelopmentalUpside = attributes is null ? .35 : Math.Clamp(.25 + attributes.Development*.25/100d, .20, .55),
            PositionPriorities = StaffWeightedPriorities(state, team, BlendBases(state, team, offense, defense, approach)),
            StaffPhilosophy = HeadPhilosophy(state, team, offense, defense)
        };
    }

    public static DynastyState Bridge(ProFranchise state, ProPlayer? arrival = null)
    {
        var players = state.Players.Where(ProDepthChart.Active).Select(item => item.Football).ToList();
        if (arrival is not null) players.Add(arrival.Football);
        return new()
        {
            DynastyName = state.Name,
            UserTeamName = state.UserTeam,
            ActiveRoster = players,
            TeamGamePlans = state.GamePlans.ToArray()
        };
    }

    public static IEnumerable<ProPlayer> Active(ProFranchise state, string team) =>
        state.Players.Where(item => item.Football.TeamName.Equals(team, StringComparison.OrdinalIgnoreCase) && ProDepthChart.Active(item));

    public static int GetPositionTarget(ProFranchise state, Position position, string team, TeamIdentity? identity = null)
    {
        identity ??= GetIdentity(state, team);
        var target = BaseTarget(position, identity.Offense, identity.Defense);
        var philosophy = identity.StaffPhilosophy;
        if (philosophy is null || philosophy.Weight <= 0 ||
            (philosophy.Offense == identity.Offense && philosophy.Defense == identity.Defense))
            return target;
        return RosterPlanService.BlendTarget(target, BaseTarget(position, philosophy.Offense, philosophy.Defense), philosophy.Weight);
    }

    private static int BaseTarget(Position position, OffensivePlaybook offense, DefensivePlaybook defense) => position switch
    {
        Position.FB when offense is OffensivePlaybook.PowerRun or OffensivePlaybook.Flexbone => 1,
        Position.FB => 0,
        Position.QB => 2,
        Position.OL => 8,
        Position.RB => offense is OffensivePlaybook.PowerRun ? 4 : 3,
        Position.WR => offense switch
        {
            OffensivePlaybook.AirRaid => 7,
            OffensivePlaybook.Spread => 6,
            OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone => 4,
            _ => 5
        },
        Position.TE => 3,
        Position.CB => defense == DefensivePlaybook.Nickel ? 5 : 4,
        Position.DE => defense == DefensivePlaybook.ThreeFour ? 3 : 4,
        Position.DT => 4,
        Position.OLB => defense == DefensivePlaybook.ThreeFour ? 4 : defense == DefensivePlaybook.Nickel ? 5 : 3,
        Position.MLB => defense == DefensivePlaybook.ThreeFour ? 3 : 4,
        Position.FS => 2,
        Position.SS => 2,
        Position.K or Position.P => 1,
        _ => 2
    };

    private static IReadOnlyDictionary<Position, int> BlendBases(
        ProFranchise state, string team, OffensivePlaybook offense, DefensivePlaybook defense, DefensiveApproach approach)
    {
        var primary = RosterPlanService.GetPositionPriorities(offense, defense, approach);
        var philosophy = HeadPhilosophy(state, team, offense, defense);
        if (philosophy is not { Weight: > 0 }) return primary;
        return RosterPlanService.BlendPriorities(
            primary,
            RosterPlanService.GetPositionPriorities(philosophy.Offense, philosophy.Defense, approach),
            philosophy.Weight);
    }

    /// <summary>
    /// Adapts identity weights to the head coach philosophy only when it differs from the club baseline;
    /// season-zero staff and unchanged hires therefore keep the catalog identity exactly.
    /// </summary>
    private static StaffPhilosophy? HeadPhilosophy(ProFranchise state, string team, OffensivePlaybook offense, DefensivePlaybook defense)
    {
        var club = state.League.Teams.FirstOrDefault(item => item.Name.Equals(team, StringComparison.OrdinalIgnoreCase));
        if (club is null) return null;
        var head = state.Coaches.FirstOrDefault(item => item.TeamId == club.Id && item.Role == StaffRole.HeadCoach && !item.Vacant);
        if (head is null) return null;
        var weight = ProTeamPhilosophy.Influence(head);
        if (weight <= 0) return null;
        var baseline = ProTeamStrategyCatalog.TryGet(club.Id, out var strategy) ? strategy : null;
        var philosophyOffense = ProTeamPhilosophy.ParseOffense(head.PreferredOffense);
        var philosophyDefense = ProTeamPhilosophy.ParseDefense(head.PreferredDefense);
        var offenseApplies = philosophyOffense is not null && philosophyOffense != baseline?.Offense;
        var defenseApplies = philosophyDefense is not null && philosophyDefense != baseline?.Defense;
        if (!offenseApplies && !defenseApplies) return null;
        return new StaffPhilosophy(
            offenseApplies ? philosophyOffense!.Value : offense,
            defenseApplies ? philosophyDefense!.Value : defense,
            weight);
    }

    public static IReadOnlyDictionary<Position, int> GetPositionTargets(ProFranchise state, string team)
    {
        var identity = GetIdentity(state, team);
        return Enum.GetValues<Position>().ToDictionary(position => position, position => GetPositionTarget(state, position, team, identity));
    }

    public static int GetPositionPriorities(ProFranchise state, string team, Position position) =>
        GetIdentity(state, team).PositionPriorities.TryGetValue(position, out var rating) ? rating : 100;

    public static int Need(ProFranchise state, string team, Position position)
    {
        var active = Active(state, team).Where(item => item.Football.CurrentInjury is null && item.Football.Position == position).Count();
        return Math.Max(0, GetPositionTarget(state, position, team) - active);
    }

    private static int GetPositionStaffRating(ProFranchise state, string team, Position position)
    {
        var club = state.League.Teams.FirstOrDefault(item => item.Name.Equals(team, StringComparison.OrdinalIgnoreCase));
        if (club is null) return 65;
        var coaches = state.Coaches.Where(item => item.TeamId == club.Id && !item.Vacant).ToArray();
        var head = coaches.FirstOrDefault(item => item.Role == StaffRole.HeadCoach);
        var role = position switch
        {
            Position.K or Position.P => StaffRole.SpecialTeamsCoordinator,
            Position.QB or Position.RB or Position.FB or Position.WR or Position.TE or Position.OL => StaffRole.OffensiveCoordinator,
            _ => StaffRole.DefensiveCoordinator
        };

        int Rating(ProCoach? coach) => coach is null ? 65 : position switch
        {
            Position.K or Position.P => ProStaff.Profile(coach).Attributes.SpecialTeams,
            Position.QB or Position.RB or Position.FB or Position.WR or Position.TE or Position.OL => ProStaff.Profile(coach).Attributes.OffensePlaycalling,
            _ => ProStaff.Profile(coach).Attributes.DefenseScheme
        };

        var coordinator = coaches.FirstOrDefault(item => item.Role == role);
        var coordinatorRating = Rating(coordinator);
        if (head is null) return coordinatorRating;
        var headRating = Rating(head);
        return (int)Math.Round(coordinatorRating * .8 + headRating * .2);
    }

    private static IReadOnlyDictionary<Position, int> StaffWeightedPriorities(
        ProFranchise state, string team, IReadOnlyDictionary<Position, int> priorities) => priorities.ToDictionary(
            pair => pair.Key,
            pair =>
            {
                var staff = GetPositionStaffRating(state, team, pair.Key);
                var influence = Math.Clamp(1 + (staff - 65) / 90d, .75, 1.35);
                return Math.Clamp((int)Math.Round(pair.Value * influence), 40, 180);
            });

    public static double Fit(ProFranchise state, string team, ProPlayer player) =>
        RosterPlanService.GetFit(Bridge(state), team, player.Football);

    public static double CapEfficiency(ProFranchise state, ProPlayer player, long annualSalary)
    {
        var priority = GetPositionPriorities(state, player.Football.TeamName, player.Football.Position) / 100d;
        var ageCurve = player.Age <= 25 ? 1.15 : player.Age <= 30 ? 1.00 : Math.Max(.65, 1 - (player.Age - 30)*.035);
        var value = (player.Football.OverallRating + player.Football.PotentialRating*.35) * ageCurve / 1.35d;
        var costIndex = 40 + 60*Math.Log10(1 + annualSalary/1_000_000d);
        return Math.Clamp(value / Math.Max(1, costIndex) * priority * 100d, 1, 100);
    }

    public static double RetentionScore(ProFranchise state, ProPlayer player, long annualSalary)
    {
        var priority = Priority(state, player.Football.TeamName, player);
        var efficiency = CapEfficiency(state, player, annualSalary);
        var ageCurve = player.Age <= 27 ? 1.05 : player.Age <= 32 ? 1.00 : Math.Max(.70, 1 - (player.Age - 32) * .02);
        var headroom = Math.Clamp(ProDevelopment.Headroom(player) / 40d, 0, 1.5);
        return priority * ageCurve * (1 + headroom * .08) + efficiency * .32;
    }

    public static double AcquisitionScore(ProFranchise state, string team, ProPlayer player, long annualSalary, bool isDraft = false)
    {
        var priority = Priority(state, team, player);
        var efficiency = CapEfficiency(state, player, isDraft ? Math.Min(annualSalary, 5_000_000) : annualSalary);
        return priority*.68 + efficiency*.32;
    }    public static double Priority(ProFranchise state, string team, ProPlayer player)
    {
        var plan = CreatePlan(state, team);
        return plan.Priority(player);
    }

    public static ProRosterPlan CreatePlan(ProFranchise state, string team) => new(state, team);
}

public sealed class ProRosterPlan
{
    private readonly ProFranchise _state;
    private readonly string _team;
    private readonly TeamIdentity _identity;
    private readonly IReadOnlyDictionary<Position, ProPlayer[]> _positions;

    internal ProRosterPlan(ProFranchise state, string team)
    {
        _state = state;
        _team = team;
        _identity = ProRosterPlanService.GetIdentity(state, team);
        _positions = ProRosterPlanService.Active(state, team)
            .GroupBy(item => item.Football.Position)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.Football.OverallRating).ToArray());
    }

    public int Need(Position position, IEnumerable<ProPlayer>? roster = null)
    {
        var count = (roster ?? _positions.GetValueOrDefault(position, []))
            .Where(item => item.Football.Position == position)
            .Count(item => ProDepthChart.Active(item));
        return Math.Max(0, ProRosterPlanService.GetPositionTarget(_state, position, _team, _identity) - count);
    }

    public int StarterGrade(Position position)
    {
        var players = _positions.GetValueOrDefault(position, []);
        var starters = Math.Max(1, position switch
        {
            Position.OL => 5,
            Position.CB or Position.WR => 3,
            Position.DE or Position.OLB or Position.RB => 2,
            _ => 1
        });
        return players.ElementAtOrDefault(starters - 1)?.Football.OverallRating ?? 55;
    }

    public double Fit(ProPlayer player) =>
        RosterPlanService.GetFit(ProRosterPlanService.Bridge(_state), _team, player.Football);

    public double Priority(ProPlayer player, bool includeTalent = true)
    {
        var position = player.Football.Position;
        var positional = ProRosterPlanService.GetPositionPriorities(_state, _team, position) / 100d;
        var fit = Fit(player);
        var need = Need(position);
        var upgrade = includeTalent ? player.Football.OverallRating - StarterGrade(position) : 0;
        var talentVersusFit = _identity.TalentVersusFit;
        var headroom = Math.Clamp(ProDevelopment.Headroom(player) / 40d, 0, 1.5);
        var upside = ProDevelopment.Headroom(player) / 100d * _identity.DevelopmentalUpside;
        return Math.Clamp(
            positional * (fit + upgrade*talentVersusFit) +
            positional * need * 8 + headroom*upside, 1, 100);
    }
}












