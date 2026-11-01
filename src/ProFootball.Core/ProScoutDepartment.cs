using DynastyManager.Core.Models;
using System.Security.Cryptography;
using System.Text;
namespace ProFootball.Core;

public sealed record ProScout(string Team,ScoutStaff Staff,bool Regional);

/// <summary>Persisted generated scouting staff, separate from football coaches and from prospect target slots.</summary>
public static class ProScoutDepartment
{
    static readonly string[] First=["Blake","Evan","Isaiah","Nolan","Grant","Miles","Colin","Owen"];
    static readonly string[] Last=["Hart","Mercer","Wallace","Pierce","Caldwell","Kim","Ortiz","Vaughn"];
    public static ProFranchise Ensure(ProFranchise s)
    {
        var scouts=s.Scouts.ToList();
        foreach(var team in s.League.Teams)
        {
            if(scouts.Any(c=>c.Team==team.Name))continue;
            var baseline=(int)s.Coaches.Where(c=>c.TeamId==team.Id&&!c.Vacant).Select(c=>c.Coaching?.TalentEvaluation??c.Development).DefaultIfEmpty(65).Average();
            for(var i=0;i<3;i++)
            {
                var bytes=SHA256.HashData(Encoding.UTF8.GetBytes($"{s.Id:N}-{team.Id}-scout-{i}"));
                int Rating(int salt)=>Math.Clamp(baseline+bytes[salt]%17-8,35,95);
                scouts.Add(new(team.Name,new ScoutStaff{ScoutId=new Guid(bytes.AsSpan(0,16)),FullName=First[bytes[16]%First.Length]+" "+Last[bytes[17]%Last.Length],
                    TalentEvaluation=Rating(18),PotentialEvaluation=Rating(19),RegionalKnowledge=Rating(20),WorkRate=65,
                    Attributes=new(){Evaluation=Rating(18),Communication=Rating(21),RegionalKnowledge=Rating(20),NationalKnowledge=Rating(19)}},i<2));
            }
        }
        return scouts.Count==s.Scouts.Count?s:s with{Scouts=scouts};
    }
    public static IReadOnlyList<ProScout> Team(ProFranchise s,string team)=>Ensure(s).Scouts.Where(c=>c.Team==team).ToArray();
    // Regional and national scouts collaborate on every report; player-origin regions are not invented.
    public static int Rating(ProFranchise s,string team)=>(int)Math.Round(Team(s,team).Select(c=>(double)ScoutAttributes.FromLegacy(c.Staff).PredictionQuality(c.Regional)).DefaultIfEmpty(65).Average());
}
