using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class StaffAttributeService
{
    public static bool IsMedical(StaffRole role) => role is StaffRole.MedicalTrainingDirector or StaffRole.StrengthConditioningDirector;
    public static bool IsFootballCoach(StaffRole role) => role is StaffRole.HeadCoach or StaffRole.OffensiveCoordinator or StaffRole.DefensiveCoordinator or StaffRole.SpecialTeamsCoordinator;
    // Missing new objects are old saves/imports: preserve their recorded abilities without rerolling identities or ratings.
    public static CoachingAttributes Coaching(StaffMember? c) => c?.Coaching ?? (c is null ? new() : new()
    {
        Leadership=c.Leadership,Development=c.PlayerDevelopment,
        OffensePlaycalling=c.Role==StaffRole.OffensiveCoordinator?c.Scheme:c.GameManagement,
        OffenseDevelopment=c.PlayerDevelopment,DefenseScheme=c.Scheme,DefenseDevelopment=c.PlayerDevelopment,
        SpecialTeams=c.SpecialTeams,TalentEvaluation=c.TalentEvaluation>0?c.TalentEvaluation:c.Recruiting
    });
    public static MedicalAttributes Medical(StaffMember? c) => c?.MedicalRatings ?? (c is null ? new() : new()
        {Medical=c.Medical,Training=c.PlayerDevelopment,Conditioning=c.Conditioning,Prevention=c.Medical,Nutrition=c.Conditioning});
    public static StaffMember Apply(StaffMember c,CoachingAttributes a,int recruiting) => c with
    {
        Coaching=a,Leadership=a.Leadership,PlayerDevelopment=a.Development,GameManagement=a.OffensePlaycalling,
        Scheme=c.Role==StaffRole.OffensiveCoordinator?a.OffensePlaycalling:a.DefenseScheme,
        SpecialTeams=a.SpecialTeams,TalentEvaluation=a.TalentEvaluation,Recruiting=Math.Clamp(recruiting,0,100)
    };
    public static StaffMember Apply(StaffMember c,MedicalAttributes a) => c with
        {MedicalRatings=a,Medical=a.Medical,PlayerDevelopment=a.Training,Conditioning=a.Conditioning};
    public static StaffMember ApplyScout(StaffMember c,ScoutAttributes a) => c with
        {ScoutingRatings=a,TalentEvaluation=a.Evaluation,PotentialEvaluation=a.NationalKnowledge,RegionalKnowledge=a.RegionalKnowledge,StaffManagement=a.Communication};
    public static int Overall(StaffMember c) => IsMedical(c.Role) ? Medical(c).Overall()
        : IsFootballCoach(c.Role) ? Coaching(c).Overall(c.Role,c.Recruiting)
        : (int)Math.Round(ScoutAttributes.FromLegacy(c).Values().Average(v=>v.Rating));
}
