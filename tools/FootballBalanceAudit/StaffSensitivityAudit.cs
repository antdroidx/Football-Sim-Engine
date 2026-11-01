using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;

// Same saved roster, schedule, schemes and seeds in each arm. Only staff abilities change.
static class StaffSensitivityAudit
{
    public static async Task Run(int count, string output)
    {
        if(count < 32) throw new ArgumentOutOfRangeException(nameof(count), "Use at least 32 matchups.");
        var state=ProLeagueService.Create("Seattle Seahawks",applyOpeningTrades:false);
        var names=state.League.Teams.Select(t=>t.Name).Order().ToArray();
        var roster=state.Players.Where(p=>ProDepthChart.Active(p)&&ProReserve.MedicallyAvailable(state,p)).Select(p=>p.Football).ToArray();
        CoachingAttributes Skills(int value)=>new(){Leadership=value,Development=value,OffensePlaycalling=value,
            OffenseDevelopment=value,DefenseScheme=value,DefenseDevelopment=value,SpecialTeams=value,TalentEvaluation=value};
        ProFranchise Rated(Func<string,int> rating)=>state with{Coaches=state.Coaches.Select(c=>ProStaff.Apply(c,Skills(rating(c.TeamId)))).ToList()};
        var neutral=Rated(_=>65);
        var totals=new Dictionary<string,List<ScheduledGame>>();
        var invalid=0;
        foreach(var arm in new[]{"current","neutral65","favored95-v-opponent35"})
        {
            var games=new List<ScheduledGame>();
            for(var i=0;i<count;i++)
            {
                var home=names[i%names.Length];var away=names[(i%names.Length+1+i/names.Length) % names.Length];
                var favored=i%2==0?home:away;
                var favoredId=state.League.Teams.Single(t=>t.Name==favored).Id;
                var s=arm=="current"?state:arm=="neutral65"?neutral:Rated(id=>id==favoredId?95:35);
                var game=new ScheduledGame{GameId="staff-"+i,SeasonYear=2026,Week=1,HomeTeamName=home,AwayTeamName=away,SimulationSeed=1000+i*7919};
                var session=GameDayEngine.Start(game,ProLeagueService.Profile(s,home),ProLeagueService.Profile(s,away),roster,GameDayMode.Simulate,rules:FootballRules.Professional);
                GameDayEngine.SimulateToEnd(session);games.Add(GameDayEngine.Finish(session));
                invalid+=session.Events.Count(e=>GameDayEngine.IsScrimmage(e)&&
                    (e.OffensePersonnel.Count!=11||e.DefensePersonnel.Count!=11||e.OffensePersonnel.Distinct().Count()!=11||e.DefensePersonnel.Distinct().Count()!=11));
            }
            totals[arm]=games;
            Console.WriteLine($"{arm}: {games.Count} games complete");
        }
        double Margin(ScheduledGame g,int i)=>(i%2==0?1:-1)*(g.HomeScore!.Value-g.AwayScore!.Value);
        var summaries=totals.Select(pair=>{
            var stats=pair.Value.SelectMany(g=>new[]{g.HomeStats!,g.AwayStats!}).ToArray();
            return new {Arm=pair.Key,Games=pair.Value.Count,Points=pair.Value.Average(g=>(g.HomeScore!.Value+g.AwayScore!.Value)/2d),
                Yards=stats.Average(s=>s.TotalYards),CompletionPct=100d*stats.Sum(s=>s.PassCompletions)/stats.Sum(s=>s.PassAttempts),
                FavoredMargin=pair.Value.Select(Margin).Average(),FavoredWinPct=100d*pair.Value.Select(Margin).Count(m=>m>0)/count};
        }).ToArray();
        var young=state.Players.Where(p=>!p.Retired&&p.Age<=24).ToArray();
        var growth=new List<object>();
        var violations=0;
        foreach(var rating in new[]{40,65,95})
        {
            var players=young;var rng=new Random(20261101);
            for(var year=0;year<=5;year++)
            {
                violations+=players.Count(p=>p.Football.OverallRating is <40 or >99||p.Football.OverallRating>p.Football.PotentialRating);
                growth.Add(new{StaffQuality=rating,Year=year,Players=players.Length,Mean=players.Average(p=>p.Football.OverallRating),
                    ElitePct=100d*players.Count(p=>p.Football.OverallRating>=90)/players.Length,Min=players.Min(p=>p.Football.OverallRating),Max=players.Max(p=>p.Football.OverallRating)});
                if(year<5)players=players.Select(p=>ProDevelopment.AgeOneYear(p,rng,rating)).ToArray();
            }
        }
        var current=summaries.Single(s=>s.Arm=="current");var baseline=summaries.Single(s=>s.Arm=="neutral65");var extreme=summaries.Single(s=>s.Arm=="favored95-v-opponent35");
        var findings=new List<string>();
        if(invalid!=0)findings.Add("Invalid personnel units");
        if(violations!=0)findings.Add("Player overall outside limits or above potential");
        if(current.Points is <18 or >32||current.Yards is <280 or >450||current.CompletionPct is <54 or >72)findings.Add("Current league mean outside broad historical audit bands");
        if(Math.Abs(current.Points-baseline.Points)>6)findings.Add("Current staff shifts scoring by more than six points per team/game");
        if(Math.Abs(extreme.FavoredMargin-baseline.FavoredMargin)>21)findings.Add("Extreme staff shifts favored margin by more than three touchdowns");
        if(extreme.FavoredMargin<=baseline.FavoredMargin)findings.Add("Extreme coaching did not improve the paired favored margin");
        var coaching=state.Coaches.Where(c=>!c.Vacant).Select(c=>new{
            c.Name,c.TeamId,Role=c.Role.ToString(),HistoryRating=ProCoachHistoryCatalog.Rating(c),
            ProfileRating=ProStaff.Profile(c).Coaching,Basis=ProCoachHistoryCatalog.Basis(c)}).ToArray();
        var report=new{Mode="staff-sensitivity",FranchiseId=state.Id,MatchupsPerArm=count,Games=count*3,
            InvalidPersonnelSnaps=invalid,RatingViolations=violations,GameArms=summaries,FiveYearDevelopment=growth,Findings=findings,
            InitialCoaching=coaching,
            Limit="Engineering smoke bands, not a fitted NFL forecast. Initial franchise staff metadata varies; arms share the same roster and seeds. Growth cohort ages without replacement; full-career runs cover roster replenishment."};
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        await File.WriteAllTextAsync(output,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine(JsonSerializer.Serialize(new{report.Games,report.InvalidPersonnelSnaps,
            report.RatingViolations,report.GameArms,report.Findings,Report=output}));
        if(findings.Count!=0)throw new InvalidOperationException(string.Join("; ",findings));
    }
}
