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
        var scouts=s.Scouts.Select(Normalize).ToList();
        var changed=!scouts.SequenceEqual(s.Scouts);
        foreach(var team in s.League.Teams)
        {
            var teamScouts=scouts.Where(c=>c.Team==team.Name).ToList();
            var usedNames=teamScouts.Select(c=>c.Staff.FullName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var attempt=0;
            while(teamScouts.Count<3)
            {
                var regional=teamScouts.Count(c=>c.Regional)<2;
                var bytes=SHA256.HashData(Encoding.UTF8.GetBytes($"{s.Id:N}|{s.Year}|{team.Id}|scout|{regional}|{attempt++}"));
                var identity=ProTeamStrategyCatalog.TryGet(team.Id,out var strategy)?strategy:null;
                var baseline=identity?.ScoutingQuality??70;
                int Rating(int salt)=>Math.Clamp(baseline+bytes[salt]%13-6,48,88);
                var first=First[bytes[16]%First.Length];var lastIndex=bytes[17]%Last.Length;var last=Last[lastIndex];
                while(!usedNames.Add(first+" "+last)){lastIndex=(lastIndex+1)%Last.Length;last=Last[lastIndex];}
                var attributes=new ScoutAttributes{Evaluation=Rating(18),Communication=Rating(21),RegionalKnowledge=Rating(20),NationalKnowledge=Rating(19)};
                var scout=new ProScout(team.Name,new ScoutStaff{
                    ScoutId=new Guid(bytes.AsSpan(0,16)),FullName=first+" "+last,
                    Age=31+bytes[22]%31,CareerYears=1+bytes[23]%20,TenureYears=bytes[24]%7,GrowthPotential=1+bytes[25]%5,
                    TalentEvaluation=attributes.Evaluation,PotentialEvaluation=attributes.NationalKnowledge,
                    RegionalKnowledge=attributes.RegionalKnowledge,WorkRate=attributes.Communication,Attributes=attributes},regional);
                teamScouts.Add(scout);scouts.Add(scout);changed=true;
            }
        }
        return changed?s with{Scouts=scouts}:s;
    }

    public static ProFranchise AdvanceSeason(ProFranchise s)
    {
        var survivors=new List<ProScout>();
        var retirements=s.StaffRetirements.ToList();
        foreach(var scout in s.Scouts.Select(Normalize))
        {
            var role=scout.Regional?"Regional scout":"National scout";
            var age=scout.Staff.Age+1;
            var attributes=ScoutAttributes.FromLegacy(scout.Staff);
            var growthChance=Math.Clamp(12+scout.Staff.GrowthPotential*8-Math.Max(0,age-50)*2,4,52);
            var declineChance=Math.Clamp((age-55)*4,0,60);
            int Shift(string key)
            {
                var roll=ProStaffRandom.Create(s.Id,s.Year,ProStaffRandom.Salt(scout.Staff.ScoutId),scout.Team,$"scout-{key}-{age}")%100;
                return roll<growthChance?1:roll>=100-declineChance?-1:0;
            }
            attributes=new ScoutAttributes{
                Evaluation=Math.Clamp(attributes.Evaluation+Shift("evaluation"),35,96),
                Communication=Math.Clamp(attributes.Communication+Shift("communication"),35,96),
                RegionalKnowledge=Math.Clamp(attributes.RegionalKnowledge+Shift("regional"),35,96),
                NationalKnowledge=Math.Clamp(attributes.NationalKnowledge+Shift("national"),35,96)};
            var retirementChance=Math.Clamp((age-60)*4,0,80);
            var retire=age>=79||age>=60&&ProStaffRandom.Create(s.Id,s.Year,0,scout.Staff.ScoutId.ToString("N"),"scout-retirement")%100<retirementChance;
            if(retire)
            {
                retirements.Add(new(s.Year,scout.Team,StaffRole.ChiefScout,scout.Staff.FullName,age,"Scouting",
                    $"Retired as {role.ToLowerInvariant()}"));
                continue;
            }
            survivors.Add(scout with{Staff=scout.Staff with{
                Age=age,CareerYears=scout.Staff.CareerYears+1,TenureYears=scout.Staff.TenureYears+1,
                Attributes=attributes,TalentEvaluation=attributes.Evaluation,PotentialEvaluation=attributes.NationalKnowledge,
                RegionalKnowledge=attributes.RegionalKnowledge,WorkRate=attributes.Communication}});
        }
        return Ensure(s with{Scouts=survivors,StaffRetirements=retirements});
    }

    static ProScout Normalize(ProScout scout)
    {
        var attributes=ScoutAttributes.FromLegacy(scout.Staff);
        return scout with{Staff=scout.Staff with{
            Age=scout.Staff.Age>0?scout.Staff.Age:32+scout.Staff.ScoutId.ToByteArray()[0]%31,
            CareerYears=scout.Staff.CareerYears>0?scout.Staff.CareerYears:1+scout.Staff.ScoutId.ToByteArray()[1]%20,
            TenureYears=Math.Max(0,scout.Staff.TenureYears),GrowthPotential=scout.Staff.GrowthPotential<=0?3:scout.Staff.GrowthPotential,
            Attributes=attributes,TalentEvaluation=attributes.Evaluation,PotentialEvaluation=attributes.NationalKnowledge,
            RegionalKnowledge=attributes.RegionalKnowledge,WorkRate=attributes.Communication}};
    }

    public static IReadOnlyList<ProScout> Team(ProFranchise s,string team)=>Ensure(s).Scouts.Where(c=>c.Team==team).ToArray();
    // Regional and national scouts collaborate on every report; player-origin regions are not invented.
    public static int Rating(ProFranchise s,string team)=>(int)Math.Round(Team(s,team).Select(c=>(double)ScoutAttributes.FromLegacy(c.Staff).PredictionQuality(c.Regional)).DefaultIfEmpty(70).Average());
}
