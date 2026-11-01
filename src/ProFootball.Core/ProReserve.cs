using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using System.Reflection;
using System.Text.Json;
namespace ProFootball.Core;

/// <summary>Activation of the existing imported reserve pool; no source medical dates are inferred.</summary>
public sealed record ProReserveInjury(int Year,string Team,int GamesAtPlacement,int MinimumGames,bool SeasonEnding,string Description,string? Source,bool Estimated);

public static class ProReserve
{
    sealed record Report(string Name,string Team,string ReportedOn,string Description,string Source);
    sealed record Pack(int Version,int Year,string ObservedOn,List<Report> Players);
    static readonly Lazy<Pack> Reports=new(()=>{
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ProFootball.Core.pro-opening-reserve-injuries.json")!;
        var pack=JsonSerializer.Deserialize<Pack>(stream)!;
        if(pack.Version!=1||pack.Year!=2026||pack.Players.Select(p=>(p.Name,p.Team)).Distinct().Count()!=pack.Players.Count||pack.Players.Any(p=>!Uri.TryCreate(p.Source,UriKind.Absolute,out _)))throw new InvalidDataException("Invalid opening reserve injury reports");
        return pack;
    });
    public static ProFranchise InitializeOpening(ProFranchise s)
    {
        if(s.ReservePolicyVersion>=1)return s;
        var original=ProLeagueService.Snapshot().Players.Select(p=>p.Football.PlayerId).ToHashSet();
        var reports=Reports.Value.Players.ToDictionary(p=>(p.Name,p.Team));
        return s with{ReservePolicyVersion=1,Players=s.Players.Select(p=>{
            if(s.Year!=2026||!p.Reserve||p.Retired||p.ReserveInjury is not null||!original.Contains(p.Football.PlayerId)||s.UnsignedRookies.Any(r=>r.PlayerId==p.Football.PlayerId)
                ||p.RosterStatus.Trim().ToUpperInvariant() is "EXE" or "SUSP")return p;
            reports.TryGetValue((p.Football.FullName,p.Football.TeamName),out var report);
            return p with{RosterStatus="IR",ReserveInjury=new(s.Year,p.Football.TeamName,0,4,report is not null,report?.Description??"Reserve injury",report?.Source,report is null)};
        }).ToList()};
    }
    static int Played(ProFranchise s,string team)=>s.Games.Count(g=>g.Phase==ProPhase.RegularSeason&&g.Football.HasPlayed&&g.Football.InvolvesTeam(team));
    public static string? InjuryReason(ProFranchise s,ProPlayer p)
    {
        if(p.ReserveInjury is {} ir&&ir.Year==s.Year)
        {
            if(ir.SeasonEnding)return "Season-ending IR · "+ir.Description+" · unavailable for the remainder of "+ir.Year+".";
            var remaining=Math.Max(0,ir.MinimumGames-(Played(s,ir.Team)-ir.GamesAtPlacement));
            if(remaining>0)return $"IR · {remaining} team games remaining before return eligibility (four-game minimum).";
        }
        return p.Football.CurrentInjury is {WeeksRemaining:>0} injury?$"Not medically available: {injury.WeeksRemaining} weeks of recovery remain.":null;
    }
    public static bool MedicallyAvailable(ProFranchise s,ProPlayer p)=>InjuryReason(s,p) is null;
    public static string Availability(ProFranchise s,ProPlayer p)=>InjuryReason(s,p)??(p.Reserve?"Reserve list · "+p.RosterStatus+" · medically available":"Available");
    public static ProFranchise AfterGame(ProFranchise s,ProGame completed)
    {
        if(completed.Phase!=ProPhase.RegularSeason)return s;
        var players=s.Players.ToList();
        foreach(var team in new[]{completed.Football.HomeTeamName,completed.Football.AwayTeamName})
        {
            var recovery=ProSportsMedicine.RecoveryBonus(s,team);
            for(var i=0;i<players.Count;i++)
            {
                var p=players[i];
                if(p.Football.TeamName!=team||p.Football.CurrentInjury is not {} injury)continue;
                var roll=MedicalRoll(s,completed,p,"recovery");
                var decrement=recovery>0&&roll<recovery*20?2:recovery<0&&roll<25?0:1;
                var remaining=Math.Max(0,injury.WeeksRemaining-decrement);
                players[i]=p with{Football=p.Football with{CurrentInjury=remaining<=0?null:injury with{WeeksRemaining=remaining}}};
            }

            // Prevention is intentionally rare per team-game. It mirrors College by reducing the chance from the
            // medical staff overall while individual medical attributes shape that overall.
            var prevention=ProSportsMedicine.PreventionAdjustment(s,team);
            var injuryChance=Math.Clamp(400-prevention,250,550); // basis points, 2.5% to 5.5%
            if(MedicalRoll(s,completed,null,"injury",team)%10000>=injuryChance)continue;
            var candidate=players.Where(p=>p.Football.TeamName==team&&!p.Retired&&!p.PracticeSquad&&!p.Reserve&&
                    p.Football.CurrentInjury is null&&completed.Football.PlayerStats.Any(stat=>stat.PlayerId==p.Football.PlayerId))
                .OrderBy(p=>MedicalRoll(s,completed,p,"candidate"))
                .FirstOrDefault();
            if(candidate is null)continue;
            var severityRoll=MedicalRoll(s,completed,candidate,"severity");
            var severity=(severityRoll%100) switch{<70=>InjurySeverity.Minor,<92=>InjurySeverity.Moderate,_=>InjurySeverity.Major};
            var rawDuration=severity switch{InjurySeverity.Minor=>1+severityRoll%2,InjurySeverity.Moderate=>2+severityRoll%4,_=>4+severityRoll%5};
            var duration=Math.Max(1,rawDuration-Math.Max(0,ProSportsMedicine.RecoveryBonus(s,team)));
            var body=(InjuryBodyArea)(MedicalRoll(s,completed,candidate,"body")%Enum.GetValues<InjuryBodyArea>().Length);
            var injuryId=new Guid(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                $"{s.Id:N}|{completed.Football.GameId:N}|{candidate.Football.PlayerId:N}|injury")).AsSpan(0,16));
            var index=players.FindIndex(p=>p.Football.PlayerId==candidate.Football.PlayerId);
            players[index]=candidate with{Football=candidate.Football with{CurrentInjury=new PlayerInjury{
                InjuryId=injuryId,SeasonYear=s.Year,StartWeek=s.Week,BodyArea=body,Severity=severity,
                InitialWeeks=duration,WeeksRemaining=duration,IsMedicalRedshirt=false}}};
        }
        return s with{Players=players};
    }

    static int MedicalRoll(ProFranchise state,ProGame game,ProPlayer? player,string purpose,string? team=null)
    {
        var key=$"{state.Id:N}|{game.Football.GameId:N}|{player?.Football.PlayerId:N}|{team}|{state.Week}|{purpose}";
        var bytes=System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        return BitConverter.ToInt32(bytes,0)&int.MaxValue;
    }
    public static string? ActivationReason(ProFranchise s,string team,Guid id)
    {
        var p=s.Players.FirstOrDefault(p=>p.Football.PlayerId==id);
        if(p is null||p.Football.TeamName!=team)return "This player does not belong to this team.";
        if(p.Retired)return "Retired players cannot be activated.";
        if(!p.Reserve)return "This player is not on the reserve list.";
        if(s.UnsignedRookies.Any(r=>r.PlayerId==id))return "Sign this rookie's reserved contract before activation.";
        if(InjuryReason(s,p) is {} medical)return medical;
        var status=p.RosterStatus.Trim().ToUpperInvariant();
        // These source codes do not establish reinstatement or a completed return window.
        if(status is not ("" or "RES" or "INA" or "DEV")&&!(status=="IR"&&p.ReserveInjury is { } ir&&ir.Year==s.Year&&!ir.SeasonEnding))return "League clearance or reserve-return eligibility must be established for status "+status+".";
        var c=s.Contracts.LastOrDefault(c=>c.Active&&c.PlayerId==id&&c.Team==team);
        if(c is null||!c.Years.Any(y=>y.Year==s.Year&&!y.IsVoid))return "A signed playing contract for this season is required.";
        if(ProRoster.CountForLimit(s,team)>=ProRoster.Limit(s))return ProRoster.IsCamp(s)?"The 90-player camp roster is full. Make room before activation.":"The 53-player active roster is full. Make room before activation.";
        var next=Proposal(s,p,c);
        if(ProEconomy.Room(next,team)-ProEconomy.Pending(next,team)<0)return "Insufficient cap space after activation and pending offers. Make room before activation.";
        return null;
    }
    static ProFranchise Proposal(ProFranchise s,ProPlayer p,ProContract c)
    {
        var id=p.Football.PlayerId;
        var rows=c.Years.Select(y=>y.Year==s.Year?y with{Salary=Math.Max(y.Salary,ProFinance.ContractMinimum(s,p,c,y.Year))}:y).ToList();
        return s with{InternationalPracticeDesignations=s.InternationalPracticeDesignations.Where(d=>d.PlayerId!=id).ToList(),Players=s.Players.Select(x=>x.Football.PlayerId==id?x with{Reserve=false,PracticeSquad=false,RosterStatus="ACT",Football=x.Football with{DepthChartOrder=99}}:x).ToList(),Contracts=s.Contracts.Select(x=>x==c?c with{Years=rows}:x).ToList()};
    }
    public static ProFranchise Activate(ProFranchise s,string team,Guid id)
    {
        if(ActivationReason(s,team,id) is {} reason)throw new InvalidOperationException(reason);
        var p=s.Players.Single(p=>p.Football.PlayerId==id);
        var c=s.Contracts.Last(c=>c.Active&&c.PlayerId==id&&c.Team==team);
        var next=Proposal(s,p,c);
        return ProEconomy.Log(next,$"{team} activated {p.Football.FullName} from reserve.");
    }
}
