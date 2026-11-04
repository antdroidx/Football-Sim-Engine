using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using System.Text.Json;
using Xunit;
namespace DynastyManager.Core.Tests;
public class StaffAttributeTests
{
    [Fact] public void CatalogsUseRequestedOrderAndSeparateMedicalAndScouting()
    {
        Assert.Equal(new[]{"Leadership","Development","Offense Playcalling","Offense Development","Defense Scheme","Defense Development","Special Teams","Talent Evaluation"},new CoachingAttributes().Values().Select(v=>v.Name));
        Assert.Equal(new[]{"Medical","Training","Conditioning","Prevention","Nutrition"},new MedicalAttributes().Values().Select(v=>v.Name));
        Assert.Equal(new[]{"Evaluation","Communication","Regional Knowledge","National Knowledge"},new ScoutAttributes().Values().Select(v=>v.Name));
    }
    [Fact] public void OldStaffRetainRecordedSkillsAndNewAttributesSurviveSaveRoundtrip()
    {
        var old=new StaffMember{FullName="Coach",TeamName="Team",Role=StaffRole.OffensiveCoordinator,Scheme=81,PlayerDevelopment=72,Leadership=90,Recruiting=87};
        Assert.Equal(81,StaffAttributeService.Coaching(old).OffensePlaycalling);
        Assert.Equal(72,StaffAttributeService.Coaching(old).OffenseDevelopment);
        var edited=StaffAttributeService.Apply(old,new(){OffensePlaycalling=55,OffenseDevelopment=94,DefenseDevelopment=35},87);
        var loaded=JsonSerializer.Deserialize<StaffMember>(JsonSerializer.Serialize(edited))!;
        Assert.Equal(edited.Coaching,loaded.Coaching);
        Assert.Equal(87,loaded.Recruiting);
        Assert.True(loaded.Coaching!.DevelopmentFor(Position.QB)>loaded.Coaching.DevelopmentFor(Position.CB));
        Assert.Equal(55,loaded.Coaching.OffensePlaycalling);
    }
    [Fact] public void AllScoutAttributesContributeToRelevantPredictionQuality()
    {
        var baseline=ScoutAttributes.FromValues(new[]{40,40,40,40});
        Assert.True((baseline with{Evaluation=90}).PredictionQuality(true)>baseline.PredictionQuality(true));
        Assert.True((baseline with{Communication=90}).PredictionQuality(true)>baseline.PredictionQuality(true));
        Assert.True((baseline with{RegionalKnowledge=90}).PredictionQuality(true)>baseline.PredictionQuality(true));
        Assert.True((baseline with{NationalKnowledge=90}).PredictionQuality(false)>baseline.PredictionQuality(false));
        Assert.Equal(baseline.PredictionQuality(false),(baseline with{RegionalKnowledge=90}).PredictionQuality(false));
    }
    [Fact] public void BetterCollegeScoutsNarrowEstimatesWithoutChangingProspectOrProgress()
    {
        var recruit=new HighSchoolRecruit{RecruitId=Guid.NewGuid(),FullName="Prospect",Position=Position.QB,TrueOverallRating=70,PotentialRating=70};
        var s=new DynastyState{DynastyId=Guid.Parse("61616161-7272-8383-9494-a5a5a5a5a5a5"),DynastyName="Test",UserTeamName="Team",SeasonYear=2027,HighSchoolRecruitingPool=[recruit]};
        DynastyState Rated(int rating)=>s with{ScoutingStaff=[new ScoutStaff{FullName="Scout",Attributes=ScoutAttributes.FromValues(new[]{rating,rating,rating,rating})}]};
        var weak=Rated(35);var strong=Rated(95);
        var a=InteractiveRecruitingService.GetScoutedOverallRange(weak,RecruitingSource.HighSchool,recruit.RecruitId);
        var b=InteractiveRecruitingService.GetScoutedOverallRange(strong,RecruitingSource.HighSchool,recruit.RecruitId);
        Assert.True(b.Maximum-b.Minimum<a.Maximum-a.Minimum);
        var c=InteractiveRecruitingService.GetScoutedPotentialRange(weak,RecruitingSource.HighSchool,recruit.RecruitId);
        var d=InteractiveRecruitingService.GetScoutedPotentialRange(strong,RecruitingSource.HighSchool,recruit.RecruitId);
        Assert.True(d.Maximum-d.Minimum<c.Maximum-c.Minimum,$"weak={c} strong={d}; evaluators={ScoutingDepartmentService.PredictionRating(weak,RecruitingSource.HighSchool,recruit.RecruitId)},{ScoutingDepartmentService.PredictionRating(strong,RecruitingSource.HighSchool,recruit.RecruitId)}");
        Assert.Equal(weak.HighSchoolRecruitingPool,strong.HighSchoolRecruitingPool);
        Assert.Empty(strong.RecruitingInteractions);
    }

    [Fact] public void CollegeStrategyCatalogCoversEveryTeamAndKeepsSourceIdentities()
    {
        Assert.Equal(138,CollegeTeamStrategyCatalog.All.Count);
        Assert.Equal(138,CollegeTeamStrategyCatalog.All.Select(item=>item.TeamName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.True(CollegeTeamStrategyCatalog.TryGet("Air Force",out var airForce));
        Assert.Equal(OffensivePlaybook.Flexbone,airForce.Offense);
        Assert.Equal(DefensivePlaybook.ThreeFour,airForce.Defense);
        var actual=CollegeTeamStrategyCatalog.All.Select(identity=>new Team{Name=identity.TeamName,
            Abbreviation=identity.TeamName[..Math.Min(3,identity.TeamName.Length)].ToUpperInvariant(),ConferenceName="Test",Prestige=65}).ToArray();
        var initialized=CollegeTeamStrategyCatalog.InitializePlans(new DynastyState{DynastyName="Strategy",UserTeamName="Air Force"},actual);
        Assert.Equal(138,initialized.TeamGamePlans.Count);
        Assert.All(actual,team=>Assert.Contains(initialized.TeamGamePlans,plan=>plan.TeamName==team.Name));
    }

    [Fact] public void CollegeStaffAttributesArePrestigeSensitiveCanonicalAndVaried()
    {
        var teams=new[]
        {
            Team("User",60),Team("High",96),Team("Low",35),Team("Mid",68)
        };
        var state=ProgramPrestigeService.EnsureInitialPrestige(new DynastyState{
            DynastyId=Guid.Parse("91939797-6161-5050-4949-484841414141"),DynastyName="Staff diversity",
            UserTeamName="User",SeasonYear=2026,LeagueTeams=teams},teams);
        state=StaffManagementService.EnsureLeagueStaff(state,teams);
        foreach(var role in new[]{StaffRole.HeadCoach,StaffRole.OffensiveCoordinator,StaffRole.DefensiveCoordinator,StaffRole.SpecialTeamsCoordinator})
        {
            var generated=state.Staff.Where(item=>item.Role==role).ToArray();
            Assert.All(generated,item=>Assert.NotNull(item.Coaching));
            var vectors=generated.Select(item=>string.Join(',',item.Coaching!.Values().Select(value=>value.Rating))).ToArray();
            Assert.Equal(vectors.Length,vectors.Distinct().Count());
        }
        Assert.All(state.Staff.Where(item=>StaffAttributeService.IsMedical(item.Role)),item=>Assert.NotNull(item.MedicalRatings));
        Assert.All(state.Staff.Where(item=>item.Role==StaffRole.ChiefScout),item=>Assert.NotNull(item.ScoutingRatings));
        int CoachMean(string team)
        {
            var average=StaffManagementService.GetTeamStaff(state,team)
                .Where(item=>StaffAttributeService.IsFootballCoach(item.Role))
                .SelectMany(item=>item.Coaching!.Values()).Average(value=>value.Rating);
            return (int)Math.Round(average);
        }
        Assert.True(CoachMean("High")>CoachMean("Low"));
        Assert.InRange(state.Staff.Where(item=>StaffAttributeService.IsFootballCoach(item.Role)).SelectMany(item=>item.Coaching!.Values()).Average(value=>value.Rating),60,72);
    }

    [Fact] public void CollegeCoachesAndScoutsAgeAndRetiringScoutsAreReplaced()
    {
        var teams=new[]{Team("User",64),Team("Rival",72)};
        var state=ProgramPrestigeService.EnsureInitialPrestige(new DynastyState{
            DynastyId=Guid.Parse("51515151-6262-7373-8484-959595959595"),DynastyName="Lifecycle",
            UserTeamName="User",SeasonYear=2026,LeagueTeams=teams},teams);
        state=StaffManagementService.EnsureLeagueStaff(state,teams);
        var user=teams[0];
        state=ScoutingDepartmentService.EnsureDepartment(state,user);
        var coachAges=state.Staff.ToDictionary(item=>item.StaffId,item=>item.Age);
        var scoutAges=state.ScoutingStaff.ToDictionary(item=>item.ScoutId,item=>item.Age);
        var retiringScout=state.ScoutingStaff[0] with{Age=90};
        state=state with{ScoutingStaff=state.ScoutingStaff.Select(item=>item.ScoutId==retiringScout.ScoutId?retiringScout:item).ToArray()};
        var scoutCount=state.ScoutingStaff.Count;
        Assert.Equal(0,state.StaffLastAdvancedSeasonYear);
        state=StaffManagementService.AdvanceSeason(state);
        Assert.Equal(2026,state.StaffLastAdvancedSeasonYear);
        Assert.All(state.Staff.Where(item=>coachAges.ContainsKey(item.StaffId)),
            item=>Assert.Equal(coachAges[item.StaffId]+1,item.Age));
        Assert.DoesNotContain(state.ScoutingStaff,item=>item.ScoutId==retiringScout.ScoutId);
        Assert.Equal(scoutCount,state.ScoutingStaff.Count);
        Assert.All(state.ScoutingStaff,item=>Assert.True(item.Age>0));
        Assert.All(state.ScoutingStaff.Where(item=>scoutAges.ContainsKey(item.ScoutId)&&item.ScoutId!=retiringScout.ScoutId),
            item=>Assert.Equal(scoutAges[item.ScoutId]+1,item.Age));
    }

    private static Team Team(string name,int prestige)=>new(){Name=name,Abbreviation=name[..Math.Min(3,name.Length)].ToUpperInvariant(),ConferenceName="Test",Prestige=prestige,LegacyRegionId=1};
}
