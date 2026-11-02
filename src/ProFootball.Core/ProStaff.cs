using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

public static class ProStaff
{
    public sealed record CoachAbilities(CoachingAttributes Attributes, StaffRole Role, string OffenseScheme, string DefenseScheme)
    {
        public int Offense => Attributes.OffensePlaycalling;
        public int Defense => Attributes.DefenseScheme;
        public int SpecialTeams => Attributes.SpecialTeams;
        public int Development => Attributes.Development;
        public int Leadership => Attributes.Leadership;
        public int Coaching => Attributes.Overall(Role);
    }
    public static ProCoach Apply(ProCoach coach, CoachingAttributes a) => coach with
    { Coaching=a,Leadership=a.Leadership,Development=a.Development,OffenseRating=a.OffensePlaycalling,
      DefenseRating=a.DefenseScheme,SpecialTeamsRating=a.SpecialTeams,Rating=a.Overall(coach.Role) };

    // Existing saves only carry the defaults (55 / Balanced). Derive a stable, name-keyed profile so
    // every coach reads differently without rewriting save records.
    public static CoachAbilities Profile(ProCoach c)
    {
        var team = ProTeamStrategyCatalog.TryGet(c.TeamId, out var identity) ? identity : null;
        var attributes = c.Coaching ?? ProTeamStrategyCatalog.Attributes(c);
        return new CoachAbilities(attributes, c.Role,
            string.IsNullOrWhiteSpace(c.PreferredOffense) || c.PreferredOffense == "Balanced" ? team?.OffenseIdentity ?? "Balanced" : c.PreferredOffense,
            string.IsNullOrWhiteSpace(c.PreferredDefense) || c.PreferredDefense == "Balanced" ? team?.DefenseIdentity ?? "Balanced" : c.PreferredDefense);
    }
    // Position-scoped staff development quality used by Pro player growth.
    public static int DevelopmentRating(ProFranchise s, string team, Position position)
    {
        var club = s.League.Teams.FirstOrDefault(t => t.Name == team);
        if (club is null) return 65;
        var role = position switch
        {
            Position.QB or Position.RB or Position.FB or Position.WR or Position.TE or Position.OL => StaffRole.OffensiveCoordinator,
            Position.DE or Position.DT or Position.OLB or Position.MLB or Position.CB or Position.FS or Position.SS => StaffRole.DefensiveCoordinator,
            Position.K or Position.P => StaffRole.SpecialTeamsCoordinator,
            _ => StaffRole.HeadCoach
        };
        var coaches = s.Coaches.Where(c => c.TeamId == club.Id && !c.Vacant && (c.Role == role || c.Role == StaffRole.HeadCoach)).ToList();
        var coaching = coaches.Count == 0 ? 65 : coaches.Average(c => Profile(c).Attributes.DevelopmentFor(position));
        var medical = ProMedicalStaffDepartment.Get(s, team, StaffRole.StrengthConditioningDirector);
        var health = StaffAttributeService.Medical(medical);
        var strength = (health.Training + health.Conditioning + health.Nutrition) / 3d;
        return (int)Math.Round((coaching + strength) / 2d, MidpointRounding.AwayFromZero);
    }
    public static ProFranchise DevelopSeason(ProFranchise s)
    {
        if (s.StaffLifecycleYear >= s.Year) return s;
        var standings=ProStandings.Build(s).ToDictionary(r=>r.Team.Id);
        var userClub=s.League.Teams.FirstOrDefault(team=>team.Name==s.UserTeam);
        var protectedRole=ProCareer.Role(s);
        var coaches=new List<ProCoach>();
        var retirements=s.StaffRetirements.ToList();
        foreach(var original in s.Coaches)
        {
            if(original.Vacant){coaches.Add(original);continue;}
            var coach=InitializeMetadata(original,s.Id);
            if(!standings.TryGetValue(coach.TeamId,out var row)||row.Wins+row.Losses+row.Ties==0){coaches.Add(coach);continue;}
            var age=coach.Age+1;
            var target=s.Owners.FirstOrDefault(o=>o.Team==row.Team.Name)?.TargetWins??8;
            var performance=row.Wins-target;
            var performanceBonus=Math.Clamp(performance/3,-1,1);
            var attributes=Profile(coach).Attributes;
            var growthChance=Math.Clamp(5+coach.GrowthPotential*7-Math.Max(0,age-45)*2+Math.Max(0,performanceBonus)*3,3,45);
            var declineChance=Math.Clamp(Math.Max(0,age-53)*4+Math.Max(0,-performanceBonus)*3,0,65);
            int Shift(string key)
            {
                var roll=ProStaffRandom.Create(s.Id,s.Year,0,coach.TeamId+coach.Role,coach.Name+"|"+key+"|"+age)%100;
                return StaffLifecycleMath.Shift(roll, growthChance, declineChance);
            }
            var updated=attributes with
            {
                Leadership=StaffLifecycleMath.Attribute(attributes.Leadership,Shift("leadership")),
                Development=StaffLifecycleMath.Attribute(attributes.Development,Shift("development")),
                OffensePlaycalling=StaffLifecycleMath.Attribute(attributes.OffensePlaycalling,Shift("offense")),
                OffenseDevelopment=StaffLifecycleMath.Attribute(attributes.OffenseDevelopment,Shift("offense-development")),
                DefenseScheme=StaffLifecycleMath.Attribute(attributes.DefenseScheme,Shift("defense")),
                DefenseDevelopment=StaffLifecycleMath.Attribute(attributes.DefenseDevelopment,Shift("defense-development")),
                SpecialTeams=StaffLifecycleMath.Attribute(attributes.SpecialTeams,Shift("special-teams")),
                TalentEvaluation=StaffLifecycleMath.Attribute(attributes.TalentEvaluation,Shift("talent"))
            };
            var applied=Apply(coach,updated) with{
                Age=age,CareerYears=coach.CareerYears+1,TenureYears=coach.TenureYears+1,LastDevelopmentYear=s.Year};
            var isUserRole=userClub is not null&&coach.TeamId==userClub.Id&&coach.Role==protectedRole&&s.Career?.Unemployed!=true;
            var retirementChance=Math.Clamp((age-60)*4+Math.Max(0,-performanceBonus)*4,0,82);
            var retire=!isUserRole&&(age>=80||age>=60&&ProStaffRandom.Create(s.Id,s.Year,0,
                coach.TeamId+coach.Name,"coach-retirement")%100<retirementChance);
            if(retire)
            {
                retirements.Add(new(s.Year,row.Team.Name,coach.Role,coach.Name,age,"Coaching",
                    performanceBonus<0?"Retired after a difficult season at the age of "+age:"Retired at the age of "+age));
                coaches.Add(applied with{Name="Vacant",Vacant=true,Title="Vacant franchise role"});
            }
            else coaches.Add(applied);
        }

        var candidates=s.CoachCandidates.ToList();
        var occupied=candidates.Select(c=>c.Name).Concat(coaches.Select(c=>c.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach(var player in s.Players.Where(p=>p.Retired&&!occupied.Contains(p.Football.FullName)&&
                    (p.Football.Skills?.Leadership??p.Football.AwarenessRating)>=75)
                .OrderByDescending(p=>(p.Football.Skills?.Leadership??0)+p.Football.AwarenessRating)
                .Take(8))
        {
            candidates.Add(new(player.Football.FullName,"RET","Former pro player · coaching track","Retired player"));
            occupied.Add(player.Football.FullName);
        }
        string[] first=["Marcus","Jordan","Evan","Miles","Cameron","Darius","Nolan","Isaiah"];
        string[] last=["Brooks","Hayes","Reed","Bennett","Warren","Price","Coleman","Grant"];
        for(var index=0;index<12;index++)
        {
            var seed=ProStaffRandom.Create(s.Id,s.Year,index,"coach-candidate-pool","staff");
            var name=first[seed%first.Length]+" "+last[seed/17%last.Length];
            if(occupied.Add(name))
                candidates.Add(new(name,"FA",index%3==0?"Position coach · promotion track":"Coordinator · experienced", "Generated staff pool"));
        }

        var advanced=s with{Coaches=coaches,CoachCandidates=candidates,StaffRetirements=retirements,StaffLifecycleYear=s.Year};
        advanced=ProMedicalStaffDepartment.AdvanceSeason(advanced);
        return ProScoutDepartment.AdvanceSeason(advanced);
    }

    public static ProFranchise ManageCpu(ProFranchise s)
    {
        foreach (var team in s.League.Teams.Where(t => t.Name != s.UserTeam || s.Career?.Unemployed == true))
        {
            var owner = s.Owners.FirstOrDefault(o => o.Team == team.Name);
            if (owner?.Fired == true)
            {
                var hc = s.Coaches.FirstOrDefault(c => c.TeamId == team.Id && c.Role == StaffRole.HeadCoach);
                if (hc is not null && !hc.Vacant) s = Fire(s, team.Name, StaffRole.HeadCoach);
                s = s with { Owners = s.Owners.Select(o => o.Team == team.Name ? o with { Security = 60, Fired = false } : o).ToList() };
            }
            foreach (var role in new[] { StaffRole.HeadCoach, StaffRole.OffensiveCoordinator, StaffRole.DefensiveCoordinator, StaffRole.SpecialTeamsCoordinator })
            {
                var current = s.Coaches.FirstOrDefault(c => c.TeamId == team.Id && c.Role == role);
                if (current is not null && !current.Vacant) continue;
                // A source-declared DC vacancy can represent the head coach calling defense.
                if (current?.Title.Contains("calls defense") == true) continue;
                var candidate = s.CoachCandidates.OrderBy(c => c.Name).FirstOrDefault(); if (candidate is null) break;
                s = Hire(s, team.Name, role, candidate.Name);
            }
        }
        return s;
    }

    public static ProCoach InitializeMetadata(ProCoach coach, Guid franchiseId)
    {
        if (coach.Age > 0) return coach;
        unchecked
        {
            var hash = 2166136261u;
            foreach (var ch in $"{franchiseId:N}|{coach.TeamId}|{coach.Role}|{coach.Name}".ToUpperInvariant())
                hash = (hash ^ ch) * 16777619u;
            var age = coach.Role == StaffRole.HeadCoach ? 38 + (int)(hash % 25) : 32 + (int)(hash % 31);
            return coach with
            {
                Age = age,
                CareerYears = Math.Clamp(age - 27 - (int)(hash / 17 % 7), 1, 35),
                TenureYears = (int)(hash / 31 % 9),
                GrowthPotential = age <= 42 ? 5 : age <= 55 ? 4 : age <= 63 ? 2 : 1
            };
        }
    }
    public static ProFranchise Hire(ProFranchise s, string team, StaffRole role, string name)
    {
        var club = s.League.Teams.Single(t => t.Name == team); var candidate = s.CoachCandidates.FirstOrDefault(c => c.Name == name) ?? throw new InvalidOperationException("This coach is unavailable.");
        var coaches = s.Coaches.ToList(); var old = coaches.FirstOrDefault(c => c.TeamId == club.Id && c.Role == role); var pool = s.CoachCandidates.Where(c => c.Name != name).ToList();
        if (old is not null && !old.Vacant) pool.Add(new(old.Name, club.Id, old.Title, old.Source));
        coaches.RemoveAll(c => c.TeamId == club.Id && c.Role == role); ProCoach hired = new(club.Id, role, name, role.ToString() + " · franchise hire", candidate.Source) { Rating = 65, Development = 65 }; hired = InitializeMetadata(hired, s.Id) with { Coaching = ProTeamStrategyCatalog.Attributes(hired, s.Id) }; var profile = Profile(hired); coaches.Add(hired with { OffenseRating = profile.Offense, DefenseRating = profile.Defense, SpecialTeamsRating = profile.SpecialTeams, Leadership = profile.Leadership, PreferredOffense = profile.OffenseScheme, PreferredDefense = profile.DefenseScheme });
        return ProEconomy.Log(s with { Coaches = coaches, CoachCandidates = pool }, $"{team} hired {name} as {role}.");
    }
    public static ProFranchise Fire(ProFranchise s, string team, StaffRole role)
    {
        var club = s.League.Teams.Single(t => t.Name == team); var old = s.Coaches.First(c => c.TeamId == club.Id && c.Role == role);
        if (old.Vacant) throw new InvalidOperationException("The role is already vacant.");
        return ProEconomy.Log(s with { Coaches = s.Coaches.Select(c => c == old ? c with { Name = "Vacant", Vacant = true, Title = "Vacant franchise role" } : c).ToList(), CoachCandidates = s.CoachCandidates.Append(new(old.Name, club.Id, old.Title, old.Source)).DistinctBy(c => c.Name).ToList() }, $"{team} dismissed {old.Name}.");
    }
}
