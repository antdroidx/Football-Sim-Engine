using System.Security.Cryptography;
using System.Text;
using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class StaffManagementService
{
    private static readonly StaffRole[] RequiredRoles = Enum.GetValues<StaffRole>();
    private static readonly string[] FirstNames =
    {
        "Alex", "Jordan", "Marcus", "Derrick", "Chris", "Taylor", "Cameron",
        "Andre", "Ryan", "Morgan", "Casey", "Devin", "Jamie", "Darius"
    };
    private static readonly string[] LastNames =
    {
        "Bennett", "Cole", "Reed", "Navarro", "Hayes", "Foster", "Brooks",
        "Sullivan", "Price", "Turner", "Morris", "Grant", "Parker", "Davis"
    };

    public static DynastyState EnsureLeagueStaff(
        DynastyState state,
        IEnumerable<Team> teams,
        string? preserveVacanciesForTeam = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(teams);

        var teamArray = teams
            .OrderBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        state = EnsureUserHeadCoach(state, teamArray);

        var staff = state.Staff
            .Where(item =>
                !(item.Role == state.UserCoachRole &&
                  item.TeamName.Equals(
                      state.UserTeamName,
                      StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var team in teamArray)
        {
            foreach (var role in RequiredRoles)
            {
                if (role == state.UserCoachRole &&
                    team.Name.Equals(
                        state.UserTeamName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (staff.Any(item =>
                        item.TeamName.Equals(team.Name, StringComparison.OrdinalIgnoreCase) &&
                        item.Role == role))
                {
                    continue;
                }

                if ((state.UserCoachRole == StaffRole.HeadCoach || !team.Name.Equals(state.UserTeamName, StringComparison.OrdinalIgnoreCase)) && preserveVacanciesForTeam is not null &&
                    team.Name.Equals(
                        preserveVacanciesForTeam,
                        StringComparison.OrdinalIgnoreCase) &&
                    state.StaffMarketSeasonYear == state.SeasonYear)
                {
                    continue;
                }

                staff.Add(CreateStaff(state, team, role, 0));
            }
        }

        return state with { Staff = staff };
    }

    public static StaffMember? GetStaff(
        DynastyState state,
        string teamName,
        StaffRole role)
    {
        if (role == state.UserCoachRole &&
            teamName.Equals(
                state.UserTeamName,
                StringComparison.OrdinalIgnoreCase) &&
            state.UserHeadCoach is not null)
        {
            return state.UserHeadCoach;
        }

        var member = state.Staff.FirstOrDefault(item =>
            item.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase) && item.Role == role);
        if (member?.IsHeadCoachRoleAssignment == true && role != StaffRole.HeadCoach)
        {
            var head = GetStaff(state, teamName, StaffRole.HeadCoach);
            if (head is not null) return head with { Role = role, IsHeadCoachRoleAssignment = true,
                PublishedTitle = member.PublishedTitle };
        }
        return member;
    }

    public static int GetRoleRating(
        DynastyState state,
        string teamName,
        StaffRole role,
        int fallback = 60) =>
        GetStaff(state, teamName, role)?.OverallRating ?? fallback;

    public static IReadOnlyList<StaffMember> GetTeamStaff(
        DynastyState state,
        string teamName) =>
        state.Staff
            .Where(item => item.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Role)
            .Select(item => GetStaff(state, teamName, item.Role) ?? item).ToArray();

    public static StaffMember GenerateCandidate(
        DynastyState state,
        Team team,
        StaffRole role)
    {
        var current = GetStaff(state, team.Name, role);
        var salt = (current?.TenureYears ?? 0) + state.SeasonYear + 31;
        return CreateStaff(state, team, role, salt);
    }

    public static DynastyState Hire(
        DynastyState state,
        StaffMember candidate)
    {
        if ((state.UserCoachRole != StaffRole.HeadCoach || candidate.Role == state.UserCoachRole) &&
            candidate.TeamName.Equals(
                state.UserTeamName,
                StringComparison.OrdinalIgnoreCase))
        {
            return state;
        }

        var staff = state.Staff
            .Where(item => !(item.TeamName.Equals(
                                candidate.TeamName,
                                StringComparison.OrdinalIgnoreCase) &&
                            item.Role == candidate.Role))
            .Append(candidate with
            {
                TenureYears = 0,
                ContractYearsRemaining = Math.Max(2, candidate.ContractYearsRemaining)
            })
            .ToArray();

        return state with { Staff = staff };
    }

    public static DynastyState AdvanceSeason(DynastyState state)
    {
        if (state.StaffLastAdvancedSeasonYear >= state.SeasonYear)
            return state;

        var staff = state.Staff.Select(item =>
        {
            var growthSeed = SimulationSeed.Create(
                state.DynastyId,
                state.SeasonYear,
                (int)item.Role,
                item.TeamName,
                $"staff-growth-{item.StaffId:N}");
            var growthChance = Math.Clamp(
                8 + item.GrowthPotential * 8 - Math.Max(0, item.Age - 58),
                3,
                48);
            var growth = growthSeed % 100 < growthChance ? 1 : 0;
            var decline = item.Age >= 64 && growthSeed % 100 < Math.Min(35, item.Age - 58)
                ? 1
                : 0;
            var latest = state.TeamSeasonHistory
                .Where(record => record.SeasonYear == state.SeasonYear && record.TeamName.Equals(item.TeamName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.SeasonYear).FirstOrDefault();
            var performanceChange = latest is null || latest.Wins + latest.Losses == 0 ? 0 :
                latest.Wins / (double)(latest.Wins + latest.Losses) >= .68 ? 1 :
                latest.Wins / (double)(latest.Wins + latest.Losses) <= .32 ? -1 : 0;
            var netChange = Math.Clamp(growth - decline + performanceChange, -1, 1);

            return item with
            {
                Coaching = StaffAttributeService.IsFootballCoach(item.Role) ? StaffAttributeService.Coaching(item).Improve(netChange) : item.Coaching,
                MedicalRatings = StaffAttributeService.IsMedical(item.Role) ? StaffAttributeService.Medical(item).Improve(netChange) : item.MedicalRatings,
                ScoutingRatings = item.Role == StaffRole.ChiefScout ? ScoutAttributes.FromLegacy(item).Improve(netChange) : item.ScoutingRatings,
                Reputation = Clamp(item.Reputation + netChange),
                Leadership = Clamp(item.Leadership + netChange),
                Recruiting = Clamp(item.Recruiting + netChange),
                PlayerDevelopment = Clamp(item.PlayerDevelopment + netChange),
                GameManagement = Clamp(item.GameManagement + netChange),
                Scheme = Clamp(item.Scheme + netChange),
                SpecialTeams = Clamp(item.SpecialTeams + netChange),
                Medical = Clamp(item.Medical + netChange),
                Conditioning = Clamp(item.Conditioning + netChange),
                TalentEvaluation = Clamp(item.TalentEvaluation + netChange),
                PotentialEvaluation = Clamp(item.PotentialEvaluation + netChange),
                RegionalKnowledge = Clamp(item.RegionalKnowledge + netChange),
                StaffManagement = Clamp(item.StaffManagement + netChange),
                Age = item.Age + 1,
                CareerYears = item.CareerYears + 1,
                TenureYears = item.TenureYears + 1,
                ContractYearsRemaining = Math.Max(0, item.ContractYearsRemaining - 1)
            };
        }).ToArray();

        var advanced = CoachSuccessionService.ResolveRetirements(state with
        {
            Staff = staff,
            StaffLastAdvancedSeasonYear = state.SeasonYear
        });
        var userTeam = advanced.LeagueTeams.FirstOrDefault(team =>
            team.Name.Equals(advanced.UserTeamName, StringComparison.OrdinalIgnoreCase));
        return userTeam is null ? advanced : ScoutingDepartmentService.AdvanceSeason(advanced, userTeam);
    }

    public static DynastyState EnsureUserHeadCoach(
        DynastyState state,
        IEnumerable<Team> teams)
    {
        if (state.UserHeadCoach is not null)
        {
            if (state.UserHeadCoach.Role != state.UserCoachRole || !state.UserHeadCoach.TeamName.Equals(
                    state.UserTeamName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return state with
                {
                    UserHeadCoach = state.UserHeadCoach with
                    {
                        TeamName = state.UserTeamName,
                        Role = state.UserCoachRole
                    }
                };
            }

            return state;
        }

        var userTeam = teams.FirstOrDefault(team =>
            team.Name.Equals(
                state.UserTeamName,
                StringComparison.OrdinalIgnoreCase));
        if (userTeam is null)
            return state;

        var legacy = state.Staff.FirstOrDefault(item =>
            item.TeamName.Equals(
                state.UserTeamName,
                StringComparison.OrdinalIgnoreCase) &&
            item.Role == state.UserCoachRole);

        var profile = legacy is not null
            ? legacy with
            {
                FullName = "YOU",
                TeamName = state.UserTeamName,
                Role = state.UserCoachRole
            }
            : CreateUserHeadCoach(state, userTeam);

        return state with
        {
            UserHeadCoach = profile with { Role = state.UserCoachRole, IsHeadCoachRoleAssignment = false,
                StaffId = state.UserCoachRole == StaffRole.HeadCoach ? profile.StaffId : StableId(state.DynastyId, "user-head-coach"),
                SharedRoleCoaches = [], PublishedTitle = UserCoachControlService.Label(state.UserCoachRole) }
        };
    }

    private static StaffMember CreateUserHeadCoach(
        DynastyState state,
        Team team)
    {
        var prestige =
            ProgramPrestigeService.GetCurrentPrestige(state, team);
        var baseRating = Math.Clamp(
            55 + prestige / 10,
            58,
            66);

        int Attribute(string key, int adjustment = 0)
        {
            var noise = SimulationSeed.Create(
                state.DynastyId,
                state.SeasonYear,
                adjustment,
                team.Name,
                $"user-hc-{key}") % 7 - 3;

            return Math.Clamp(
                baseRating + adjustment + noise,
                50,
                75);
        }

        return new StaffMember
        {
            StaffId = StableId(
                state.DynastyId,
                "user-head-coach"),
            FullName = "YOU",
            TeamName = team.Name,
            Role = StaffRole.HeadCoach,
            Reputation = Attribute("reputation", -2),
            Leadership = Attribute("leadership", 1),
            Recruiting = Attribute("recruiting"),
            PlayerDevelopment = Attribute("development"),
            GameManagement = Attribute("game-management"),
            Scheme = Attribute("scheme"),
            SpecialTeams = 60,
            Medical = 60,
            Conditioning = 60,
            TalentEvaluation = 60,
            PotentialEvaluation = 60,
            RegionalKnowledge = 60,
            StaffManagement = Attribute("staff-management"),
            Age = 35,
            CareerYears = 0,
            GrowthPotential = 5,
            TenureYears = 0,
            ContractYearsRemaining = 4
        };
    }

    private static StaffMember CreateStaff(
        DynastyState state,
        Team team,
        StaffRole role,
        int salt)
    {
        var prestige = ProgramPrestigeService.GetCurrentPrestige(state, team);
        var seed = SimulationSeed.Create(
            state.DynastyId,
            state.SeasonYear,
            (int)role + salt,
            team.Name,
            "staff");
        var name = $"{FirstNames[seed % FirstNames.Length]} " +
                   $"{LastNames[(seed / 17) % LastNames.Length]}";

        // New dynasties target about 65 overall in College. Prestige raises the center and the dynasty seed
        // creates a new but stable staff distribution for each save.
        var programBase = Math.Clamp(
            (int)Math.Round(61 + (prestige - 65) * .20, MidpointRounding.AwayFromZero),
            52, 74);
        int Rating(string key, int adjustment = 0) => Clamp(
            programBase + adjustment +
            SimulationSeed.Create(
                state.DynastyId,
                state.SeasonYear,
                salt + (int)role,
                team.Name,
                $"staff-{key}") % 7 - 3);

        var coaching = new CoachingAttributes
        {
            Leadership = Rating("lead", role == StaffRole.HeadCoach ? 4 : role is StaffRole.OffensiveCoordinator or StaffRole.DefensiveCoordinator ? 1 : -2),
            Development = Rating("develop", role is StaffRole.HeadCoach or StaffRole.OffensiveCoordinator or StaffRole.DefensiveCoordinator ? 2 : -1),
            OffensePlaycalling = Rating("offense", role == StaffRole.OffensiveCoordinator ? 5 : role == StaffRole.HeadCoach ? 1 : -4),
            OffenseDevelopment = Rating("offense-development", role is StaffRole.HeadCoach or StaffRole.OffensiveCoordinator ? 4 : -3),
            DefenseScheme = Rating("defense", role == StaffRole.DefensiveCoordinator ? 5 : role == StaffRole.HeadCoach ? 1 : -4),
            DefenseDevelopment = Rating("defense-development", role is StaffRole.HeadCoach or StaffRole.DefensiveCoordinator ? 4 : -3),
            SpecialTeams = Rating("special-teams", role == StaffRole.SpecialTeamsCoordinator ? 7 : -3),
            TalentEvaluation = Rating("talent", role is StaffRole.HeadCoach or StaffRole.ChiefScout ? 2 : -1)
        };
        var medical = role == StaffRole.MedicalTrainingDirector
            ? new MedicalAttributes
            {
                Medical = Rating("medical", 5), Training = Rating("training", 1),
                Conditioning = Rating("conditioning", -1), Prevention = Rating("prevention", 4),
                Nutrition = Rating("nutrition", 1)
            }
            : new MedicalAttributes
            {
                Medical = Rating("medical", -2), Training = Rating("training", 3),
                Conditioning = Rating("conditioning", 5), Prevention = Rating("prevention", 1),
                Nutrition = Rating("nutrition", 3)
            };
        var scouting = new ScoutAttributes
        {
            Evaluation = Rating("evaluation", 3), Communication = Rating("communication", 1),
            RegionalKnowledge = Rating("regional", 2), NationalKnowledge = Rating("national", 1)
        };
        var recruiting = Rating("recruiting", role == StaffRole.HeadCoach ? 3 : role is StaffRole.OffensiveCoordinator or StaffRole.DefensiveCoordinator ? 1 : -3);

        var member = new StaffMember
        {
            StaffId = StableId(state.DynastyId, $"{team.Name}|{role}|{salt}"),
            FullName = name,
            TeamName = team.Name,
            Role = role,
            Reputation = Rating("rep"),
            Leadership = coaching.Leadership,
            Recruiting = recruiting,
            PlayerDevelopment = coaching.Development,
            GameManagement = coaching.OffensePlaycalling,
            Scheme = role == StaffRole.OffensiveCoordinator ? coaching.OffensePlaycalling : coaching.DefenseScheme,
            SpecialTeams = coaching.SpecialTeams,
            Medical = medical.Medical,
            Conditioning = medical.Conditioning,
            TalentEvaluation = scouting.Evaluation,
            PotentialEvaluation = scouting.NationalKnowledge,
            RegionalKnowledge = scouting.RegionalKnowledge,
            StaffManagement = scouting.Communication,
            Age = 34 + seed % 27,
            CareerYears = seed % 18,
            GrowthPotential = Math.Clamp(
                5 - Math.Max(0, (34 + seed % 27) - 42) / 8 +
                SimulationSeed.Create(
                    state.DynastyId,
                    state.SeasonYear,
                    salt + (int)role,
                    team.Name,
                    "staff-growth-potential") % 2,
                1,
                5),
            ContractYearsRemaining = 2 + seed % 4
        };

        if (StaffAttributeService.IsFootballCoach(role))
            member = StaffAttributeService.Apply(member, coaching, recruiting);
        else if (StaffAttributeService.IsMedical(role))
            member = StaffAttributeService.Apply(member, medical);
        else if (role == StaffRole.ChiefScout)
            member = StaffAttributeService.ApplyScout(member, scouting);
        return member;
    }

    private static int Clamp(int value) => Math.Clamp(value, 35, 99);

    private static Guid StableId(Guid dynastyId, string purpose) =>
        new(MD5.HashData(Encoding.UTF8.GetBytes($"{dynastyId:N}|{purpose}")));
}
