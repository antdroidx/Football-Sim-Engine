using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using System.Security.Cryptography;
using System.Text;
namespace ProFootball.Core;

/// <summary>Persisted pro sports-medicine staff, separate from coaches and scouts.</summary>
public sealed record ProMedicalStaff(string Team, StaffMember Staff);

public static class ProMedicalStaffDepartment
{
    static readonly string[] First=["Avery","Jordan","Morgan","Cameron","Riley","Taylor","Drew","Casey","Reese","Dakota","Parker","Quinn"];
    static readonly string[] Last=["Brooks","Hayes","Reed","Bennett","Morgan","Carter","Sullivan","Patel","Kim","Ortiz","Wallace","Grant"];
    public static readonly StaffRole[] Roles=[StaffRole.MedicalTrainingDirector,StaffRole.StrengthConditioningDirector];

    public static ProFranchise Ensure(ProFranchise state)
    {
        var staff=0;
        var medical=state.MedicalStaff.ToList();
        foreach(var team in state.League.Teams)
        {
            var existing=medical.Where(item=>item.Team==team.Name).Select(item=>item.Staff.Role).ToHashSet();
            foreach(var role in Roles)
            {
                if(existing.Contains(role))continue;
                medical.Add(new(team.Name,Create(state,team,role,staff++)));
            }
        }
        return medical.Count==state.MedicalStaff.Count?state:state with{MedicalStaff=medical};
    }

    public static IReadOnlyList<ProMedicalStaff> Team(ProFranchise state,string team)=>
        Ensure(state).MedicalStaff.Where(item=>item.Team==team).ToArray();

    public static StaffMember? Get(ProFranchise state,string team,StaffRole role)=>
        Team(state,team).FirstOrDefault(item=>item.Staff.Role==role)?.Staff;

    public static MedicalAttributes Attributes(ProMedicalStaff member)=>StaffAttributeService.Medical(member.Staff);

    public static ProFranchise AdvanceSeason(ProFranchise state)
    {
        var survivors=new List<ProMedicalStaff>();
        var retirements=state.StaffRetirements.ToList();
        foreach(var member in state.MedicalStaff)
        {
            var age=member.Staff.Age+1;
            var attributes=StaffAttributeService.Medical(member.Staff);
            var growthChance=Math.Clamp(12+member.Staff.GrowthPotential*7-Math.Max(0,age-45)*2,4,45);
            var declineChance=Math.Clamp((age-52)*4,0,60);
            int Shift(string key)
            {
                var roll=ProStaffRandom.Create(state.Id,state.Year,ProStaffRandom.Salt(member.Staff.StaffId),member.Team,$"medical-{key}-{age}")%100;
                return roll<growthChance?1:roll>=100-declineChance?-1:0;
            }
            var updated=new MedicalAttributes{
                Medical=Math.Clamp(attributes.Medical+Shift("medical"),35,96),
                Training=Math.Clamp(attributes.Training+Shift("training"),35,96),
                Conditioning=Math.Clamp(attributes.Conditioning+Shift("conditioning"),35,96),
                Prevention=Math.Clamp(attributes.Prevention+Shift("prevention"),35,96),
                Nutrition=Math.Clamp(attributes.Nutrition+Shift("nutrition"),35,96)};
            var retirementChance=Math.Clamp((age-60)*4,0,80);
            var retire=age>=78||age>=60&&ProStaffRandom.Create(state.Id,state.Year,0,member.Staff.StaffId.ToString("N"),"medical-retirement")%100<retirementChance;
            if(retire)
            {
                retirements.Add(new(state.Year,member.Team,member.Staff.Role,member.Staff.FullName,age,"Medical",
                    "Retired from the sports-medicine staff"));
                continue;
            }
            survivors.Add(member with{Staff=StaffAttributeService.Apply(member.Staff,updated) with{
                Age=age,CareerYears=member.Staff.CareerYears+1,TenureYears=member.Staff.TenureYears+1}});
        }
        return Ensure(state with{MedicalStaff=survivors,StaffRetirements=retirements});
    }

    static StaffMember Create(ProFranchise state,ProTeam team,StaffRole role,int ordinal)
    {
        var bytes=SHA256.HashData(Encoding.UTF8.GetBytes($"{state.Id:N}|{state.Year}|{team.Id}|medical|{role}"));
        var quality=64+bytes[0]%13;
        int Rating(int salt,int adjustment=0)=>Math.Clamp(quality+adjustment+bytes[salt]%11-5,50,88);
        var attributes=role==StaffRole.MedicalTrainingDirector
            ? new MedicalAttributes{Medical=Rating(4,4),Training=Rating(5,1),Conditioning=Rating(6,-1),Prevention=Rating(7,3),Nutrition=Rating(8,-1)}
            : new MedicalAttributes{Medical=Rating(4,-3),Training=Rating(5,4),Conditioning=Rating(6,5),Prevention=Rating(7,1),Nutrition=Rating(8,3)};
        var name=First[bytes[9]%First.Length]+" "+Last[bytes[10]%Last.Length];
        var unique=$"{name}|{team.Id}|{role}";
        var id=new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(unique)).AsSpan(0,16));
        var member=new StaffMember{StaffId=id,FullName=name,TeamName=team.Name,Role=role,Age=37+bytes[11]%24,
            CareerYears=4+bytes[12]%18,TenureYears=bytes[13]%6,ContractYearsRemaining=1+bytes[14]%4,
            GrowthPotential=1+bytes[15]%4,Reputation=quality,Medical=attributes.Medical,
            PlayerDevelopment=attributes.Training,Conditioning=attributes.Conditioning};
        return StaffAttributeService.Apply(member,attributes);
    }
}

/// <summary>Shared mapping of medical attributes into pro prevention, recovery and player development.</summary>
public static class ProSportsMedicine
{
    public static int DevelopmentRating(ProFranchise state,string team)
    {
        var strength=ProMedicalStaffDepartment.Get(state,team,StaffRole.StrengthConditioningDirector);
        var values=StaffAttributeService.Medical(strength);
        return (int)Math.Round((values.Training+values.Conditioning+values.Nutrition)/3d,MidpointRounding.AwayFromZero);
    }

    public static int Overall(ProFranchise state,string team)
    {
        var values=ProMedicalStaffDepartment.Team(state,team).Select(item=>StaffAttributeService.Medical(item.Staff).Overall()).ToArray();
        return values.Length==0?70:(int)Math.Round(values.Average(),MidpointRounding.AwayFromZero);
    }

    // Mirrors College: the medical-staff overall contributes to prevention; every medical attribute is represented.
    public static int PreventionAdjustment(ProFranchise state,string team)=>
        (int)Math.Round((Overall(state,team)-70)*1.25,MidpointRounding.AwayFromZero);

    public static int RecoveryBonus(ProFranchise state,string team)=>Overall(state,team) switch
    {
        >=82=>2,
        >=72=>1,
        <55=>-1,
        _=>0
    };
}
