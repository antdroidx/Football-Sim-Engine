using DynastyManager.Core.Models;
namespace ProFootball.Core;

public sealed record CpuRosterHealth(int Active, int Minimum, long CheapestFill, long Available,
    IReadOnlyDictionary<Position, int> PositionNeeds)
{
    public long FundingShortfall => Math.Max(0, CheapestFill - Available);
    public bool MeetsMinimum => Active >= Minimum && PositionNeeds.Values.All(n => n == 0);
}

/// <summary>Current roster survival precedes discretionary spending and unearned future carryover.</summary>
public static class ProCpuRosterSafety
{
    public const int MinimumActive = 45;
    public static long RookieReserve(ProFranchise s,string team) => s.UnsignedRookies.Where(r=>r.Team==team).Sum(r=>r.Salary+(r.Bonus+3)/4);
    static IEnumerable<ProPlayer> Candidates(ProFranchise s) => s.Players.Where(p =>
        p.Football.TeamName == "Free agent" && !p.Retired && ProReserve.MedicallyAvailable(s,p) &&
        !s.Contracts.Any(c => c.Active && c.PlayerId == p.Football.PlayerId) &&
        !s.Waivers.Any(w => w.PlayerId == p.Football.PlayerId) && !s.Bids.Any(b => b.PlayerId == p.Football.PlayerId));

    public static CpuRosterHealth Health(ProFranchise s, string team)
    {
        var active = ProRoster.Active(s,team);
        var slots = Math.Max(0,MinimumActive-active);
        var prices = Candidates(s).Select(p => ProCpu.SigningSalary(s,p,1)).Order().Take(slots).ToArray();
        // An exhausted market is a real deficit, not permission to invent free players or cap room.
        var cost = prices.Length < slots ? long.MaxValue : prices.Sum();
        return new(active,MinimumActive,cost,Math.Max(0,ProEconomy.Room(s,team)-ProEconomy.Pending(s,team)),
            Enum.GetValues<Position>().ToDictionary(p => p,p => ProCpu.Need(s,team,p)));
    }

    public static ProFranchise EnsureMinimum(ProFranchise s, string team, Func<ProPlayer,bool>? eligible = null)
    {
        for (var attempt=0; attempt<53 && ProRoster.Active(s,team)<53; attempt++)
        {
            var active=ProRoster.Active(s,team);
            var needs=Enum.GetValues<Position>().ToDictionary(p=>p,p=>ProCpu.Need(s,team,p));
            if(active>=MinimumActive && needs.Values.All(n=>n==0))break;
            var room=ProEconomy.Room(s,team)-ProEconomy.Pending(s,team)-RookieReserve(s,team);
            var candidate=Candidates(s).Where(p => (eligible is null || eligible(p)) &&
                (active<MinimumActive || needs[p.Football.Position]>0) && ProCpu.SigningSalary(s,p,1)<=room)
                .OrderBy(p=>ProCpu.SigningSalary(s,p,1))
                .ThenByDescending(p=>needs[p.Football.Position]>0).ThenBy(p=>p.Football.PlayerId).FirstOrDefault();
            if(candidate is null)break;
            var next=ProEconomy.Sign(s,new(candidate.Football.PlayerId,team,1,ProCpu.SigningSalary(s,candidate,1),0,s.Week));
            if(next==s)break;
            s=next;
        }
        return s;
    }

    public static ProFranchise ReleaseForBudget(ProFranchise s, ProPlayer player)
    {
        var team=player.Football.TeamName;
        var released=ProEconomy.Release(s,team,player.Football.PlayerId,false);
        if(ProRoster.Active(released,team)>=MinimumActive && ProCpu.StaffDepthSurvives(s,player))return released;
        var replacement=Candidates(released).Where(p=>p.Football.PlayerId!=player.Football.PlayerId &&
            p.Football.Position==player.Football.Position &&
            (player.Football.Position is not (Position.OL or Position.K) || player.Football.NcaaPositionCode is null || p.Football.NcaaPositionCode==player.Football.NcaaPositionCode))
            .OrderBy(p=>ProCpu.SigningSalary(released,p,1)).ThenBy(p=>p.Football.PlayerId).FirstOrDefault();
        if(replacement is null)return s;
        var salary=ProCpu.SigningSalary(released,replacement,1);
        if(ProEconomy.Room(released,team)-ProEconomy.Pending(released,team)<salary)return s;
        var replaced=ProEconomy.Sign(released,new(replacement.Football.PlayerId,team,1,salary,0,s.Week));
        return replaced!=released && ProEconomy.Room(replaced,team)>ProEconomy.Room(s,team) ? replaced : s;
    }
}
