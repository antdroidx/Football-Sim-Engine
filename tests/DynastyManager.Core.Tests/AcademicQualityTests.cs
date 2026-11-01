using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using DynastyManager.Data.Import;
using Xunit;
namespace DynastyManager.Core.Tests;

public sealed class AcademicQualityTests
{
    static Team Team(string name,int prestige,int academics=-1)=>new(){Name=name,Abbreviation=name[..Math.Min(3,name.Length)].ToUpperInvariant(),
        ConferenceName="Test",Prestige=prestige,AcademicQuality=academics,LegacyRegionId=1};

    [Fact] public void AcademicQualityUsesInstitutionValueAndFullZeroToNinetyNineScale()
    {
        Assert.Equal(98,TeamAcademicQualityCatalog.Get(Team("Stanford",0)));
        Assert.Equal(0,TeamAcademicQualityCatalog.Get(Team("Custom",0,0)));
        Assert.Equal(99,TeamAcademicQualityCatalog.Get(Team("Custom",0,99)));
        var akron=TeamAcademicQualityCatalog.Get(Team("Akron",45));
        Assert.InRange(akron,0,99);
        Assert.True(TeamAcademicQualityCatalog.Get(Team("Stanford",45))>akron);
    }

    [Fact] public void AcademicsPitchUsesTeamAcademicQualityForHighSchoolRecruits()
    {
        var recruit=new HighSchoolRecruit{RecruitId=Guid.NewGuid(),FullName="Education first",Position=Position.WR,
            StarRating=4,TrueOverallRating=78,PotentialRating=86,HomeRegion=1,HomeState=5};
        var state=new DynastyState{DynastyId=Guid.NewGuid(),DynastyName="Academics",UserTeamName="Stanford",SeasonYear=2027,
            HighSchoolRecruitingPool=[recruit]};
        Assert.Equal(98,RecruitPreferenceService.GetProgramGrade(state,Team("Stanford",70,98),
            RecruitingSource.HighSchool,recruit.RecruitId,RecruitPitchType.Academics));
        Assert.Equal(52,RecruitPreferenceService.GetProgramGrade(state,Team("Akron",45,52),
            RecruitingSource.HighSchool,recruit.RecruitId,RecruitPitchType.Academics));
    }

    [Fact] public void PrestigeCanStartAtZeroAndChangesWhileAcademicQualityRemainsStatic()
    {
        var low=Team("Low",0,51);var lookup=new Dictionary<string,Team>{[low.Name]=low};
        var state=ProgramPrestigeService.EnsureInitialPrestige(new DynastyState{DynastyName="Prestige",UserTeamName="Low",SeasonYear=2026},[low]);
        Assert.Equal(0,ProgramPrestigeService.GetCurrentPrestige(state,low));
        state=ProgramPrestigeService.ApplySeasonResults(state,lookup);
        var snapshot=state.ProgramPrestigeHistory.Single(item=>item.TeamName=="Low"&&item.SeasonYear==2026);
        Assert.InRange(snapshot.StartingPrestige,0,99);
        Assert.InRange(snapshot.EndingPrestige,0,99);
        Assert.Equal(51,TeamAcademicQualityCatalog.Get(low));
    }

    [Fact] public void ModernLeaguePersistsAcademicQualityForEveryTeam()
    {
        var legacy=CollegeTeamStrategyCatalog.All.Select(item=>Team(item.TeamName,60)).ToArray();
        var teams=ModernFbsUniverseFactory.Create(legacy);
        Assert.Equal(138,teams.Count);
        Assert.All(teams,team=>Assert.InRange(team.AcademicQuality,0,99));
        Assert.True(teams.Single(team=>team.Name=="Stanford").AcademicQuality>teams.Single(team=>team.Name=="Akron").AcademicQuality);
    }
}
