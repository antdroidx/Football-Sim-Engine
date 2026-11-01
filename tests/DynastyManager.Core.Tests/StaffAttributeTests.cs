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
        var recruit=new HighSchoolRecruit{RecruitId=Guid.NewGuid(),FullName="Prospect",Position=Position.QB,TrueOverallRating=75,PotentialRating=85};
        var s=new DynastyState{DynastyId=Guid.NewGuid(),DynastyName="Test",UserTeamName="Team",SeasonYear=2027,HighSchoolRecruitingPool=[recruit]};
        DynastyState Rated(int rating)=>s with{ScoutingStaff=[new ScoutStaff{FullName="Scout",Attributes=ScoutAttributes.FromValues(new[]{rating,rating,rating,rating})}]};
        var weak=Rated(35);var strong=Rated(95);
        var a=InteractiveRecruitingService.GetScoutedOverallRange(weak,RecruitingSource.HighSchool,recruit.RecruitId);
        var b=InteractiveRecruitingService.GetScoutedOverallRange(strong,RecruitingSource.HighSchool,recruit.RecruitId);
        Assert.True(b.Maximum-b.Minimum<a.Maximum-a.Minimum);
        var c=InteractiveRecruitingService.GetScoutedPotentialRange(weak,RecruitingSource.HighSchool,recruit.RecruitId);
        var d=InteractiveRecruitingService.GetScoutedPotentialRange(strong,RecruitingSource.HighSchool,recruit.RecruitId);
        Assert.True(d.Maximum-d.Minimum<c.Maximum-c.Minimum);
        Assert.Equal(weak.HighSchoolRecruitingPool,strong.HighSchoolRecruitingPool);
        Assert.Empty(strong.RecruitingInteractions);
    }
}

