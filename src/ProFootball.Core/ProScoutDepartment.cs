using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
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
        var unique = s.Scouts.Select(Normalize)
            .GroupBy(scout => scout.Staff.ScoutId)
            .Select(group => group.First())
            .ToList();
        var usedIds = unique.Select(scout => scout.Staff.ScoutId).ToHashSet();
        var rebuilt = new List<ProScout>();

        foreach (var team in s.League.Teams)
        {
            var existing = unique.Where(scout => scout.Team == team.Name).ToList();
            var selected = new List<ProScout>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var regionalCount = 0;
            var nationalCount = 0;
            foreach (var scout in existing)
            {
                if (!names.Add(scout.Staff.FullName)) continue;
                if (scout.Regional && regionalCount < 2)
                {
                    selected.Add(scout);
                    regionalCount++;
                }
                else if (!scout.Regional && nationalCount < 1)
                {
                    selected.Add(scout);
                    nationalCount++;
                }
            }

            var identity = ProTeamStrategyCatalog.TryGet(team.Id, out var strategy) ? strategy : null;
            var baseline = identity?.ScoutingQuality ?? 70;
            foreach (var slot in new[] { true, true, false })
            {
                if (slot && regionalCount >= 2 || !slot && nationalCount >= 1) continue;
                var scout = CreateScout(s.Id, s.Year, team, slot, baseline, usedIds, names);
                selected.Add(scout);
                usedIds.Add(scout.Staff.ScoutId);
                names.Add(scout.Staff.FullName);
                if (slot) regionalCount++; else nationalCount++;
            }

            rebuilt.AddRange(selected);
        }

        if (rebuilt.Count != s.League.Teams.Count * 3 ||
            rebuilt.Select(scout => scout.Staff.ScoutId).Distinct().Count() != rebuilt.Count ||
            s.League.Teams.Any(team => rebuilt.Count(scout => scout.Team == team.Name) != 3))
            throw new InvalidDataException("Every club must receive three unique scouts.");

        return rebuilt.SequenceEqual(s.Scouts) ? s : s with { Scouts = rebuilt };
    }

    static ProScout CreateScout(
        Guid franchiseId,
        int year,
        ProTeam team,
        bool regional,
        int baseline,
        HashSet<Guid> usedIds,
        HashSet<string> usedNames)
    {
        var slot = regional ? "regional" : "national";
        var attempt = 0;
        byte[] bytes;
        Guid id;
        do
        {
            bytes = SHA256.HashData(Encoding.UTF8.GetBytes(
                $"{franchiseId:N}|{year}|{team.Id}|scout|{slot}|{attempt++}"));
            id = new Guid(bytes.AsSpan(0, 16));
        } while (usedIds.Contains(id) && attempt < 10_000);
        if (usedIds.Contains(id)) throw new InvalidDataException("Could not allocate a unique scout identity.");

        int Rating(int salt) => Math.Clamp(baseline + bytes[salt] % 13 - 6, 48, 88);
        var names = from firstIndex in Enumerable.Range(0, First.Length)
                    from lastIndex in Enumerable.Range(0, Last.Length)
                    select First[(firstIndex + bytes[16]) % First.Length] + " " +
                           Last[(lastIndex + bytes[17]) % Last.Length];
        var name = names.FirstOrDefault(candidate => !usedNames.Contains(candidate))
            ?? throw new InvalidDataException("Could not allocate a unique scout name.");
        var attributes = new ScoutAttributes
        {
            Evaluation = Rating(18),
            Communication = Rating(21),
            RegionalKnowledge = Rating(20),
            NationalKnowledge = Rating(19)
        };
        return new ProScout(team.Name, new ScoutStaff
        {
            ScoutId = id,
            FullName = name,
            Age = 31 + bytes[22] % 31,
            CareerYears = 1 + bytes[23] % 20,
            TenureYears = bytes[24] % 7,
            GrowthPotential = 1 + bytes[25] % 5,
            TalentEvaluation = attributes.Evaluation,
            PotentialEvaluation = attributes.NationalKnowledge,
            RegionalKnowledge = attributes.RegionalKnowledge,
            WorkRate = attributes.Communication,
            Attributes = attributes
        }, regional);
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
                return StaffLifecycleMath.Shift(roll, growthChance, declineChance);
            }
            attributes=new ScoutAttributes{
                Evaluation=StaffLifecycleMath.Attribute(attributes.Evaluation,Shift("evaluation")),
                Communication=StaffLifecycleMath.Attribute(attributes.Communication,Shift("communication")),
                RegionalKnowledge=StaffLifecycleMath.Attribute(attributes.RegionalKnowledge,Shift("regional")),
                NationalKnowledge=StaffLifecycleMath.Attribute(attributes.NationalKnowledge,Shift("national"))};
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
