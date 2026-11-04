using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Seasons;
using DynastyManager.Core.Simulation;
using DynastyManager.Data.Import;
using ProFootball.Core;
static class SeasonAudit
{
    public static object ProIdentitySummary(ProFranchise s)
    {
        var plans = s.League.Teams.Select(team => new
        {
            team.Name,
            Plan = s.GamePlans.LastOrDefault(plan => plan.TeamName.Equals(team.Name, StringComparison.OrdinalIgnoreCase)),
            Baseline = ProTeamStrategyCatalog.TryGet(team.Id, out var strategy) ? strategy : null
        }).ToArray();
        return new
        {
            Year = s.Year,
            SavedPlans = plans.Count(item => item.Plan is not null),
            PlanOffenses = plans.GroupBy(item => item.Plan?.Offense ?? item.Baseline!.Offense).OrderBy(g => g.Key)
                .Select(g => new { Playbook = g.Key.ToString(), Teams = g.Count() }).ToArray(),
            PlanDefenses = plans.GroupBy(item => item.Plan?.Defense ?? item.Baseline!.Defense).OrderBy(g => g.Key)
                .Select(g => new { Playbook = g.Key.ToString(), Teams = g.Count() }).ToArray(),
            PhilosophyTeams = plans.Count(item => ProRosterPlanService.GetIdentity(s, item.Name).StaffPhilosophy is not null),
            DriftedFromCatalog = plans.Count(item => item.Plan is not null && item.Baseline is not null &&
                (item.Plan.Offense != item.Baseline.Offense || item.Plan.Defense != item.Baseline.Defense)),
            PositionTargetGapPerTeam = Math.Round(plans.Average(item =>
            {
                var targets = ProRosterPlanService.GetPositionTargets(s, item.Name);
                return targets.Where(kv => kv.Value > 0)
                    .Sum(kv => Math.Abs(ProRosterPlanService.Active(s, item.Name).Count(p => p.Football.Position == kv.Key) - kv.Value));
            }), 2),
            UserPlanOffense = plans.First(item => item.Name == s.UserTeam).Plan?.Offense.ToString() ?? "none",
            UserPlanDefense = plans.First(item => item.Name == s.UserTeam).Plan?.Defense.ToString() ?? "none"
        };
    }

    public static object CollegeIdentitySummary(DynastyState s)
    {
        var plans = s.LeagueTeams.Select(team => new
        {
            team.Name,
            Plan = s.TeamGamePlans.LastOrDefault(plan => plan.TeamName.Equals(team.Name, StringComparison.OrdinalIgnoreCase)),
            Baseline = CollegeTeamStrategyCatalog.ForTeam(team.Name)
        }).ToArray();
        return new
        {
            Year = s.SeasonYear,
            SavedPlans = plans.Count(item => item.Plan is not null),
            PlanOffenses = plans.GroupBy(item => item.Plan?.Offense ?? item.Baseline?.Offense ?? OffensivePlaybook.Balanced).OrderBy(g => g.Key)
                .Select(g => new { Playbook = g.Key.ToString(), Teams = g.Count() }).ToArray(),
            PlanDefenses = plans.GroupBy(item => item.Plan?.Defense ?? item.Baseline?.Defense ?? DefensivePlaybook.Balanced).OrderBy(g => g.Key)
                .Select(g => new { Playbook = g.Key.ToString(), Teams = g.Count() }).ToArray(),
            DriftEligibleTeams = plans.Count(item => RosterPlanService.HeadLeanPhilosophy(s, item.Name,
                item.Plan?.Offense ?? OffensivePlaybook.Balanced, item.Plan?.Defense ?? DefensivePlaybook.Balanced) is
            { Weight: >= RosterPlanService.MinimumDriftInfluence }),
            DriftedFromCatalog = plans.Count(item => item.Plan is not null && item.Baseline is not null &&
                (item.Plan.Offense != item.Baseline.Offense || item.Plan.Defense != item.Baseline.Defense)),
            PositionTargetGapPerTeam = Math.Round(plans.Average(item =>
            {
                var roster = s.ActiveRoster.Where(p => p.TeamName.Equals(item.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
                return Enum.GetValues<Position>().Where(position => RosterPlanService.GetRosterTarget(s, position, item.Name) > 0)
                    .Sum(position => Math.Abs(roster.Count(p => p.Position == position) - RosterPlanService.GetRosterTarget(s, position, item.Name)));
            }), 2),
            UserPlanOffense = plans.First(item => item.Name.Equals(s.UserTeamName, StringComparison.OrdinalIgnoreCase)).Plan?.Offense.ToString() ?? "none",
            UserPlanDefense = plans.First(item => item.Name.Equals(s.UserTeamName, StringComparison.OrdinalIgnoreCase)).Plan?.Defense.ToString() ?? "none"
        };
    }

    public static async Task Run(string mode,int years,string output)
    {
        var reports=new List<object>();var rosterChecks=new List<object>();var proIdentityChecks=new List<object>();var collegeIdentityChecks=new List<object>();var generated=mode.Contains("generated");var sampledQb=new List<double>();var sampledRb3=new List<double>();var sampledDl=new List<double>();var invalid=0;var sampledGames=0;
        void Observe(GameDaySession session)
        {
            sampledGames++;
            foreach(var snap in session.Events.Where(GameDayEngine.IsScrimmage))if(snap.OffensePersonnel.Count!=11||snap.DefensePersonnel.Count!=11||snap.OffensePersonnel.Distinct().Count()!=11||snap.DefensePersonnel.Distinct().Count()!=11)invalid++;
            foreach(var team in new[]{session.Game.HomeTeamName,session.Game.AwayTeamName})
            {
                var plays=session.Events.Where(e=>e.OffenseTeamName==team&&GameDayEngine.IsScrimmage(e)).ToArray();var qs=plays.Where(e=>e.QuarterbackId!=null).GroupBy(e=>e.QuarterbackId).Select(g=>g.Count()).OrderDescending().ToArray();if(qs.Length>0)sampledQb.Add(qs[0]/(double)qs.Sum());
                var carries=plays.Where(e=>e.BallCarrierId is Guid id&&session.PlayersById[id].Position==Position.RB&&e.Outcome is SnapOutcome.Run or SnapOutcome.Touchdown or SnapOutcome.Fumble).GroupBy(e=>e.BallCarrierId).Select(g=>g.Count()).OrderDescending().ToArray();if(carries.Length>0)sampledRb3.Add(carries.Skip(2).Sum()/(double)carries.Sum());
                var starters=PersonnelRotationService.BuildDepthChart(session.Roster,team).Where(g=>g.Key is Position.DE or Position.DT).SelectMany(g=>g.Value.Take(2)).Select(p=>p.PlayerId).ToHashSet();var dl=session.Events.Where(e=>e.DefenseTeamName==team&&GameDayEngine.IsScrimmage(e)).SelectMany(e=>e.DefensePersonnel).Where(id=>session.PlayersById[id].Position is Position.DE or Position.DT).ToArray();if(dl.Length>0)sampledDl.Add(dl.Count(id=>!starters.Contains(id))/(double)dl.Length);
            }
        }
        object Summary(int year,IEnumerable<ScheduledGame> source,IEnumerable<DynastyPlayer> roster,HashSet<Guid> original)
        {
            var games=source.Where(g=>g.HasPlayed).ToArray();var stats=games.SelectMany(g=>new[]{g.HomeStats!,g.AwayStats!}).Where(s=>s is not null).ToArray();var players=games.SelectMany(g=>g.PlayerStats).ToArray();
            var rushers=players.GroupBy(p=>p.PlayerId).Select(g=>g.Sum(p=>p.RushYards)).OrderDescending().ToArray();var passers=players.GroupBy(p=>p.PlayerId).Select(g=>g.Sum(p=>p.PassYards)).OrderDescending().ToArray();
            var r=roster.ToArray();var means=r.GroupBy(p=>p.TeamName).Select(g=>g.Average(p=>p.OverallRating)).ToArray();var mean=means.Average();
            return new{Year=year,Games=games.Length,Points=games.Average(g=>(g.HomeScore!.Value+g.AwayScore!.Value)/2d),Yards=stats.Average(s=>s.TotalYards),Passing=stats.Average(s=>s.PassYards),Rushing=stats.Average(s=>s.RushYards),PassAttempts=stats.Average(s=>s.PassAttempts),RushAttempts=stats.Average(s=>s.RushAttempts),CompletionPct=100d*stats.Sum(s=>s.PassCompletions)/stats.Sum(s=>s.PassAttempts),YardsPerRush=stats.Sum(s=>s.RushYards)/(double)stats.Sum(s=>s.RushAttempts),YardsPerPass=stats.Sum(s=>s.PassYards)/(double)stats.Sum(s=>s.PassAttempts),Sacks=stats.Average(s=>s.SacksAllowed),Turnovers=stats.Average(s=>s.Turnovers),PassLeader=passers.FirstOrDefault(),RushLeader=rushers.FirstOrDefault(),GeneratedNewcomersPlaying=players.Where(p=>!original.Contains(p.PlayerId)).Select(p=>p.PlayerId).Distinct().Count(),RosterAverage=r.Average(p=>p.OverallRating),TeamMeanStandardDeviation=Math.Sqrt(means.Average(x=>(x-mean)*(x-mean)))};
        }
        if(mode.StartsWith("pro"))
        {
            var s=ProLeagueService.Create("Seattle Seahawks",sourcedContracts:!generated,generatedPlayers:generated) with{AutoManageRookieOptions=true,AutoNegotiateExpiring=true};var original=s.Players.Select(p=>p.Football.PlayerId).ToHashSet();
            for(var cycle=0;cycle<years;cycle++)
            {
                while(s.Phase!=ProPhase.Offseason)
                {
                    if(s.Players.Any(p=>p.PracticeSquad))throw new Exception("Retired practice squad reappeared");
                    s=ProCpu.Manage(s,true);if(s.Phase==ProPhase.Preseason&&s.Week==3)s=ProCpu.Trim(s,s.UserTeam);
                    s=ProCpu.MakeRoom(s,s.UserTeam,0);
                    var game=s.Games.FirstOrDefault(g=>g.Phase==s.Phase&&g.Football.Week==s.Week&&!g.Football.HasPlayed);
                    if(game is not null){var session=ProLeagueService.Start(s,game,GameDayMode.Simulate);GameDayEngine.SimulateToEnd(session);if(s.Phase==ProPhase.RegularSeason)Observe(session);s=ProLeagueService.Finish(s,session);}
                    var before=(s.Phase,s.Week);s=ProLeagueService.Advance(s);if(before==(s.Phase,s.Week))throw new Exception("Blocked phase "+s.Year+" "+before+" "+ProSeasonEligibility.Issue(s,s.UserTeam));
                }
                rosterChecks.Add(new{Year=s.Year,PracticePlayers=s.Players.Count(p=>p.PracticeSquad),ActiveMinimum=s.League.Teams.Min(t=>ProRoster.Active(s,t.Name)),ActiveMaximum=s.League.Teams.Max(t=>ProRoster.Active(s,t.Name)),ReservePlayers=s.Players.Count(p=>p.Reserve),Fullbacks=s.Players.Count(p=>p.Football.Position==Position.FB&&p.Football.TeamName!="Free agent"&&!p.Retired)});
                var report=Summary(s.Year,s.Games.Where(g=>g.Phase==ProPhase.RegularSeason).Select(g=>g.Football),s.Players.Where(p=>ProDepthChart.Active(p)&&p.Football.TeamName!="Free agent").Select(p=>p.Football),original);reports.Add(report);Console.WriteLine(JsonSerializer.Serialize(report));proIdentityChecks.Add(ProIdentitySummary(s));
                if(cycle==years-1)break;
                s=ProOffseason.Advance(s);s=ProOffseason.Advance(s);s=ProDraft.AutoUntilUser(s,true);s=ProOffseason.Advance(s);s=ProCpu.Trim(s,s.UserTeam);s=ProOffseason.Advance(s);s=ProCpu.Manage(s,true);
                if(s.Phase!=ProPhase.Preseason)throw new Exception("Offseason failed");
            }
        }
        else
        {
            var teams=ModernFbsUniverseFactory.Create(LegacyUniverseCsvImporter.Import(File.ReadAllText("src/DynastyManager.Data/Resources/Legacy/universe.csv")).Teams).ToDictionary(t=>t.Name);
            using var reader=File.OpenText("src/DynastyManager.App/Resources/Raw/next26-aug26-2025.csv");var pack=Next26RosterCsvImporter.LoadPack(reader,teams.Keys).Players;var names=pack.Select(p=>p.TeamName).ToHashSet();var template=pack.Concat(LegacyDynastyRosterFactory.Create([],teams.Values,Guid.NewGuid()).Where(p=>!names.Contains(p.TeamName))).ToArray();
            var s=new DynastyState{DynastyName="Three-season balance",UserTeamName="Washington",SeasonYear=2026,Phase=SeasonPhase.RegularSeason,LeagueTeams=teams.Values.ToArray(),RecruitingAssistanceEnabled=true,RecruitingAssistanceWorkloadPercent=100,ActiveRoster=generated?FictionalRosterService.College(template,teams.Values.ToArray(),new Random(713)):template};
            s=RosterManagementService.NormalizeAllDepthCharts(PlayerRatingService.EnsureProfiles(s));s=CollegeTeamStrategyCatalog.InitializePlans(s,teams.Values);s=StaffManagementService.EnsureLeagueStaff(s,teams.Values);var original=s.ActiveRoster.Select(p=>p.PlayerId).ToHashSet();var user=teams[s.UserTeamName];
            for(var cycle=0;cycle<years;cycle++)
            {
                var rotation=teams.Keys.Order().ToList();var schedule=new List<ScheduledGame>();
                for(var week=1;week<=12;week++){for(var i=0;i<rotation.Count/2;i++)schedule.Add(new(){GameId=$"{s.SeasonYear}-{week}-{i}",SeasonYear=s.SeasonYear,Week=week,HomeTeamName=rotation[i],AwayTeamName=rotation[^(i+1)],SimulationSeed=s.SeasonYear*10000+week*100+i});var last=rotation[^1];rotation.RemoveAt(rotation.Count-1);rotation.Insert(1,last);}
                s=s with{Phase=SeasonPhase.RegularSeason,Schedule=schedule};
                for(var week=1;week<=12;week++)
                {
                    s=s with{Week=week};var profiles=DynastyRosterSimulationProfileBuilder.Build(s,teams.Values);var game=s.Schedule.First(g=>g.Week==week);var session=GameDayEngine.Start(game,GamePlanService.Attach(s,teams[game.HomeTeamName],profiles[game.HomeTeamName]),GamePlanService.Attach(s,teams[game.AwayTeamName],profiles[game.AwayTeamName]),s.ActiveRoster,GameDayMode.Simulate,s.DepthChartAssignments);GameDayEngine.SimulateToEnd(session);Observe(session);
                    s=WeekSimulation.SimulateCurrentRegularSeasonWeek(s,teams,profiles);s=TransferPortalMarketService.AdvanceRegularSeasonWeek(s);s=InteractiveRecruitingService.EnsurePhaseInitialized(s,user);s=CpuRecruitingService.ApplyWeeklyUserAssistance(s,user);s=InteractiveRecruitingService.ResolveCurrentPhase(s,user,teams);if(week is 3 or 6 or 9 or 12)s=PlayerDevelopmentService.ApplyMidseasonDevelopment(s);
                }
                var report=Summary(s.SeasonYear,s.Schedule,s.ActiveRoster,original);reports.Add(report);Console.WriteLine(JsonSerializer.Serialize(report));collegeIdentityChecks.Add(CollegeIdentitySummary(s));if(cycle==years-1)break;
                s=DynastyHistoryService.FinalizeSeason(s,teams);s=StaffManagementService.AdvanceSeason(s);s=ProgramPrestigeService.ApplySeasonResults(s,teams);s=s with{Phase=SeasonPhase.TransferPortal,Week=19};s=OffseasonPlayerLifecycleService.EnterTransferPortal(s);s=TransferPortalMarketService.MaterializePendingCommitments(s);s=InteractiveRecruitingService.EnsurePhaseInitialized(s,user);s=CpuRecruitingService.ApplyWeeklyUserAssistance(s,user);s=InteractiveRecruitingService.ResolveCurrentPhase(s,user,teams);s=CpuRecruitingService.ApplyPhaseAssistance(s,teams);s=SeasonProgression.Advance(s);
                s=s with{HighSchoolRecruitingPool=HighSchoolRecruitingPoolFactory.Create(s.DynastyId,s.SeasonYear,teams.Count,[])};
                for(var week=0;week<SeasonProgression.RecruitingWeekCount;week++){s=InteractiveRecruitingService.EnsurePhaseInitialized(s,user);s=CpuRecruitingService.ApplyWeeklyUserAssistance(s,user);s=InteractiveRecruitingService.ResolveCurrentPhase(s,user,teams);s=CpuRecruitingService.AdvanceCpuRecruitingWeek(s,teams);s=SeasonProgression.Advance(s);}
                s=WalkOnRosterService.FillAllTeams(s,teams);s=RosterManagementService.NormalizeAllDepthCharts(s);s=SeasonProgression.Advance(s);s=PlayerDevelopmentService.ApplyOffseasonDevelopment(s);s=SeasonProgression.Advance(s);s=StaffManagementService.AdvanceSeason(s);s=SeasonProgression.Advance(s);
            }
        }
        var final=new{Mode=mode,Seasons=reports,ProRosterSeasons=rosterChecks,ProIdentitySeasons=proIdentityChecks,CollegeIdentitySeasons=collegeIdentityChecks,PersonnelSampleGames=sampledGames,PrimaryQBSnapShare=sampledQb.Average(),ThirdAndLaterRBCarryShare=sampledRb3.Average(),BackupDLSnapShare=sampledDl.Average(),InvalidPersonnelSnaps=invalid};Directory.CreateDirectory(Path.GetDirectoryName(output)!);await File.WriteAllTextAsync(output,JsonSerializer.Serialize(final,new JsonSerializerOptions{WriteIndented=true}));
        if(invalid!=0)throw new Exception("Invalid personnel detected: "+invalid);
    }
}
