using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using DynastyManager.Data.Import;
using ProFootball.Core;

static class ModeConformanceAudit
{
    public static Task Run(int count, string output)
    {
        var rows = new List<object>();
        foreach (var pro in new[] { false, true })
        foreach (var generated in new[] { false, true })
        {
            IReadOnlyList<DynastyPlayer> roster;
            Dictionary<string, TeamSimulationProfile> profiles;
            if (pro)
            {
                var s = ProLeagueService.Create("Seattle Seahawks");
                if (generated) s = s with { Players = ProGeneratedRoster.Create(s.League, new Random(12345)) };
                roster = s.Players.Where(p => ProDepthChart.Active(p) && ProReserve.MedicallyAvailable(s,p)).Select(p=>p.Football).ToArray();
                profiles = s.League.Teams.ToDictionary(t=>t.Name,t=>ProLeagueService.Profile(s,t.Name));
            }
            else
            {
                var teams = ModernFbsUniverseFactory.Create(LegacyUniverseCsvImporter.Import(File.ReadAllText("src/DynastyManager.Data/Resources/Legacy/universe.csv")).Teams).ToArray();
                using var reader = File.OpenText("src/DynastyManager.App/Resources/Raw/next26-aug26-2025.csv");
                roster = Next26RosterCsvImporter.LoadPack(reader, teams.Select(t=>t.Name)).Players;
                if (generated) roster = FictionalRosterService.College(roster, teams, new Random(12345));
                profiles = DynastyRosterSimulationProfileBuilder.Build(new DynastyState { DynastyName="Mode audit",UserTeamName="Washington",ActiveRoster=roster },teams).ToDictionary(p=>p.Key,p=>p.Value);
            }
            var names = profiles.Keys.Where(n=>roster.Count(p=>p.TeamName==n)>=22).Order().ToArray();
            var results = new List<(GameDayMode Mode, bool Controlled, ScheduledGame Game, int Snaps, int UserCalls)>();
            for (var i=0;i<count;i++)
            {
                var game = new ScheduledGame { GameId=$"mode-{i}", SeasonYear=2026,Week=1,HomeTeamName=names[i%names.Length],AwayTeamName=names[(i+1)%names.Length],SimulationSeed=7123+i*7919 };
                GameDaySession? baseline = null;
                foreach (var controlled in new[] { false, true })
                foreach (var mode in Enum.GetValues<GameDayMode>())
                {
                    // Controlled policies are intentionally identical in all three engine paths.
                    // CPU and controlled distributions are reported separately: user choices change outcomes.
                    var s=GameDayEngine.Start(game,profiles[game.HomeTeamName],profiles[game.AwayTeamName],roster,mode,rules:pro?FootballRules.Professional:FootballRules.College);
                    if (controlled) s.ControlledTeamName=game.HomeTeamName;
                    while(!s.IsComplete && s.Events.Count<1000)
                    {
                        string? offense=null, defense=null;
                        if(controlled && !s.KickoffPending && !s.TryPending)
                        {
                            var home=s.PossessionTeamName==game.HomeTeamName;
                            var plan=(home?s.HomeProfile:s.AwayProfile).GamePlan ?? new TeamGamePlan { TeamName=s.PossessionTeamName };
                            if(home) offense=GameDayRecommendationService.Recommend(s,plan).PlayId;
                            else
                            {
                                var dp=s.HomeProfile.GamePlan ?? new TeamGamePlan { TeamName=game.HomeTeamName };
                                defense=GamePlanService.RecommendDefensivePlay(dp,s.Down,s.Distance).Id;
                            }
                        }
                        GameDayEngine.Step(s,offense,defense);
                    }
                    if(!s.IsComplete) throw new Exception("Mode did not finish");
                    foreach(var e in s.Events.Where(GameDayEngine.IsScrimmage))
                    {
                        if(e.OffensePersonnel.Count!=11 || e.DefensePersonnel.Count!=11 || e.OffensePersonnel.Distinct().Count()!=11 || e.DefensePersonnel.Distinct().Count()!=11) throw new Exception("Personnel mismatch");
                        if(e.PreSnapSeconds<0 || e.LiveBallSeconds<0 || e.ClockSeconds<0) throw new Exception("Clock bounds");
                    }
                    var finished=GameDayEngine.Finish(s);
                    if(!controlled)
                    {
                        if(baseline is null) baseline=s;
                        else if(JsonSerializer.Serialize(GameDayEngine.Finish(baseline))!=JsonSerializer.Serialize(finished)) throw new Exception("CPU mode accounting differs");
                    }
                    var stats=finished.PlayerStats;
                    if(stats.Sum(p=>p.Targets)!=stats.Sum(p=>p.CoverageTargets)) throw new Exception("Target accounting");
                    results.Add((mode,controlled,finished,s.Events.Count,s.Events.Count(e=>e.CallSource==PlayCallSource.User)));
                }
            }
            foreach(var group in results.GroupBy(r=>(r.Mode,r.Controlled)))
                rows.Add(new {League=pro?"Pro":"College",Roster=generated?"Generated":"Real",Mode=group.Key.Mode.ToString(),group.Key.Controlled,Games=group.Count(),Points=group.Average(r=>(r.Game.HomeScore!.Value+r.Game.AwayScore!.Value)/2d),Yards=group.Average(r=>(r.Game.HomeStats!.TotalYards+r.Game.AwayStats!.TotalYards)/2d),Events=group.Average(r=>r.Snaps),UserCalls=group.Sum(r=>r.UserCalls)});
        }
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output,JsonSerializer.Serialize(new {GamesPerCell=count,Rows=rows},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Mode conformance passed: {count*24} games; report {output}");
        return Task.CompletedTask;
    }
}
