using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;
using Xunit;
namespace ProFootball.Tests;

public class StaffBalanceTests
{
    static ProFranchise New()=>ProLeagueService.Create("Seattle Seahawks",generatedPlayers:true,applyOpeningTrades:false);

    [Fact] public void EveryFranchiseHasCompleteVariedStaffAndARealSchemeIdentity()
    {
        var s=New();
        Assert.Equal(32,ProTeamStrategyCatalog.All.Count);
        Assert.Equal(32,s.GamePlans.Count);
        Assert.Equal(32,s.GamePlans.Select(plan=>plan.TeamName).Distinct().Count());
        var seattle=ProTeamStrategyCatalog.Get("SEA");
        Assert.Equal(OffensivePlaybook.PlayAction,seattle.Offense);
        Assert.Equal(DefensivePlaybook.FourThree,seattle.Defense);
        Assert.Contains("play-action",seattle.OffenseIdentity,StringComparison.OrdinalIgnoreCase);
        Assert.Equal(64,s.MedicalStaff.Count);
        Assert.True(s.Scouts.Count==96,$"teams={s.League.Teams.Count}; counts={string.Join(",",s.League.Teams.Select(t=>$"{t.Id}:{s.Scouts.Count(x=>x.Team==t.Name)}"))}");
        Assert.All(s.League.Teams,team=>
        {
            Assert.Equal(2,s.MedicalStaff.Count(item=>item.Team==team.Name));
            Assert.Equal(3,s.Scouts.Count(item=>item.Team==team.Name));
            Assert.Equal(4,s.Coaches.Count(item=>item.TeamId==team.Id));Assert.True(s.Coaches.Count(item=>item.TeamId==team.Id&&!item.Vacant)>=3);
        });
        var coachingVectors=s.Coaches.Where(c=>!c.Vacant).Select(c=>string.Join(',',ProStaff.Profile(c).Attributes.Values().Select(v=>v.Rating))).ToArray();
        Assert.Equal(coachingVectors.Length,coachingVectors.Distinct().Count());
        var medicalVectors=s.MedicalStaff.Select(c=>string.Join(',',StaffAttributeService.Medical(c.Staff).Values().Select(v=>v.Rating))).ToArray();
        Assert.Equal(medicalVectors.Length,medicalVectors.Distinct().Count());
        var scoutVectors=s.Scouts.Select(c=>string.Join(',',ScoutAttributes.FromLegacy(c.Staff).Values().Select(v=>v.Rating))).ToArray();
        Assert.Equal(scoutVectors.Length,scoutVectors.Distinct().Count());
    }

    [Fact] public void ProStaffQualityCentersNearSeventyAndAttributesStayInSafeBands()
    {
        var s=New();
        var coach=s.Coaches.Where(c=>!c.Vacant).SelectMany(c=>ProStaff.Profile(c).Attributes.Values()).Select(v=>v.Rating).ToArray();
        var medical=s.MedicalStaff.SelectMany(c=>StaffAttributeService.Medical(c.Staff).Values()).Select(v=>v.Rating).ToArray();
        var scouts=s.Scouts.SelectMany(c=>ScoutAttributes.FromLegacy(c.Staff).Values()).Select(v=>v.Rating).ToArray();
        Assert.InRange(coach.Average(),65,75);
        Assert.InRange(medical.Average(),65,75);
        Assert.InRange(scouts.Average(),65,75);
        Assert.All(coach,rating=>Assert.InRange(rating,0,99));
        Assert.All(medical,rating=>Assert.InRange(rating,0,99));
        Assert.All(scouts,rating=>Assert.InRange(rating,0,99));
        Assert.True(ProTeamStrategyCatalog.All.Average(team=>team.CoachingQuality) is >=69 and <=72);
        Assert.True(ProTeamStrategyCatalog.All.Average(team=>team.ScoutingQuality) is >=68 and <=71);
    }

    [Fact] public void DifferentFranchiseSeedsCreateDifferentCoachesForTheSameClub()
    {
        var team=New().Coaches.First(c=>!c.Vacant) with{Coaching=null};
        var first=ProTeamStrategyCatalog.Attributes(team,Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var second=ProTeamStrategyCatalog.Attributes(team,Guid.Parse("22222222-2222-2222-2222-222222222222"));
        Assert.NotEqual(first,second);
    }

    [Fact] public void ObservedHistoryPlacesMikeMacdonaldInTheHighCoachBand()
    {
        var s=New();
        var coach=s.Coaches.Single(c=>c.TeamId=="SEA"&&c.Role==StaffRole.HeadCoach);
        Assert.True(ProCoachHistoryCatalog.TryCoach(coach.Name,out var history));
        Assert.Equal(34,history.Games);Assert.Equal(24,history.Wins);Assert.Equal(10,history.Losses);
        Assert.InRange(history.PointsForPerGame,25,26);Assert.InRange(history.PointsAgainstPerGame,19,20);
        var initial=ProCoachHistoryCatalog.Rating(coach);
        var profile=ProStaff.Profile(coach).Coaching;
        Assert.True(initial>=76,$"history rating {initial}");
        Assert.True(profile>=76,$"coach profile {profile}");
        Assert.Contains("SEA 2024–2025",ProCoachHistoryCatalog.Basis(coach));
    }

    [Theory]
    [InlineData("Kevin Stefanski","ATL","CLE")]
    [InlineData("John Harbaugh","NYG","BAL")]
    [InlineData("Mike McCarthy","PIT","DAL")]
    [InlineData("Robert Saleh","TEN","NYJ")]
    public void CoachHistoryKeepsTheObservedEmployerSeparateFromTheCurrentOne(
        string name,string currentTeam,string observedTeam)
    {
        var coach=New().Coaches.Single(c=>
            c.Name==name&&c.Role==StaffRole.HeadCoach);
        Assert.Equal(currentTeam,coach.TeamId);
        Assert.True(ProCoachHistoryCatalog.TryCoach(name,out var history));
        Assert.Equal(observedTeam,Assert.Single(history.Tenures).TeamId);
        Assert.Contains($"{observedTeam} ",ProCoachHistoryCatalog.Basis(coach));
    }

    [Fact] public void RobertSalehHistoryStopsWhenTheJetsDismissedHim()
    {
        var coach=New().Coaches.Single(c=>
            c.Name=="Robert Saleh"&&c.Role==StaffRole.HeadCoach);
        Assert.Equal("TEN",coach.TeamId);
        Assert.True(ProCoachHistoryCatalog.TryCoach(coach.Name,out var history));
        Assert.Equal(22,history.Games);Assert.Equal(9,history.Wins);Assert.Equal(13,history.Losses);
        Assert.Equal(16.4,history.PointsForPerGame);Assert.Equal(20.0,history.PointsAgainstPerGame);
        Assert.Equal(2023,history.FirstSeason);Assert.Equal(2024,history.LastSeason);
        Assert.Equal(65,ProCoachHistoryCatalog.Rating(coach));
        Assert.Contains("NYJ 2023–2024: 9–13 in 22",ProCoachHistoryCatalog.Basis(coach));
    }

    [Fact] public void MikeVrabelHistoryPreservesBothEmployers()
    {
        var coach=New().Coaches.Single(c=>
            c.Name=="Mike Vrabel"&&c.Role==StaffRole.HeadCoach);
        Assert.True(ProCoachHistoryCatalog.TryCoach(coach.Name,out var history));
        Assert.Equal(new[]{"TEN","NE"},history.Tenures.Select(tenure=>tenure.TeamId));
        Assert.Equal(34,history.Games);Assert.Equal(20,history.Wins);Assert.Equal(14,history.Losses);
        var basis=ProCoachHistoryCatalog.Basis(coach);
        Assert.Contains("TEN 2023: 6–11 in 17",basis);
        Assert.Contains("NE 2025: 14–3 in 17",basis);
    }

    [Fact] public void CoachingTalentEvaluationDirectlyImprovesPotentialEstimates()
    {
        var s=ProDraft.Generate(New() with{Phase=ProPhase.Offseason,OffseasonStep=OffseasonStep.Draft});
        var player=s.Prospects[0];var club=s.League.Teams.Single(t=>t.Name==s.UserTeam);
        ProFranchise Rated(int talent)
        {
            var coaching=new CoachingAttributes{Leadership=60,Development=65,OffensePlaycalling=65,OffenseDevelopment=65,
                DefenseScheme=65,DefenseDevelopment=65,SpecialTeams=60,TalentEvaluation=talent};
            return s with{Coaches=s.Coaches.Select(c=>c.TeamId==club.Id?ProStaff.Apply(c,coaching with{TalentEvaluation=c.Role switch{
                StaffRole.HeadCoach=>talent,StaffRole.OffensiveCoordinator=>Math.Clamp(talent-2,0,100),
                StaffRole.DefensiveCoordinator=>Math.Clamp(talent-1,0,100),_=>Math.Clamp(talent-3,0,100)}}):c).ToList()};
        }
        var weak=Rated(35);var strong=Rated(95);
        var weakReport=ProScouting.Evaluate(weak,s.UserTeam,player,ScoutDepth.Visit);
        var strongReport=ProScouting.Evaluate(strong,s.UserTeam,player,ScoutDepth.Visit);
        Assert.True(ProScouting.StaffRating(strong,s.UserTeam)>ProScouting.StaffRating(weak,s.UserTeam));
        Assert.True(strongReport.Confidence>=weakReport.Confidence);
        Assert.True(strongReport.PotentialConfidence>weakReport.PotentialConfidence);
        Assert.True(strongReport.PotentialHigh-strongReport.PotentialLow<=weakReport.PotentialHigh-weakReport.PotentialLow);
        Assert.InRange(strongReport.PotentialLow,40,99);Assert.InRange(strongReport.PotentialHigh,strongReport.PotentialLow,99);
    }

    [Fact] public void MedicalAttributesDriveDevelopmentPreventionAndRecoveryInBoundedWays()
    {
        var s=New();
        ProFranchise Rated(int rating)
        {
            var attributes=new MedicalAttributes{Medical=rating,Training=rating,Conditioning=rating,Prevention=rating,Nutrition=rating};
            return s with{MedicalStaff=s.MedicalStaff.Select(item=>item.Team==s.UserTeam?item with{Staff=StaffAttributeService.Apply(item.Staff,attributes)}:item).ToList()};
        }
        var low=Rated(35);var high=Rated(95);
        Assert.True(ProSportsMedicine.DevelopmentRating(high,s.UserTeam)>ProSportsMedicine.DevelopmentRating(low,s.UserTeam));
        Assert.True(ProSportsMedicine.PreventionAdjustment(high,s.UserTeam)>ProSportsMedicine.PreventionAdjustment(low,s.UserTeam),$"high={ProSportsMedicine.PreventionAdjustment(high,s.UserTeam)} low={ProSportsMedicine.PreventionAdjustment(low,s.UserTeam)}");
        Assert.True(ProSportsMedicine.RecoveryBonus(high,s.UserTeam)>ProSportsMedicine.RecoveryBonus(low,s.UserTeam));
        Assert.InRange(ProStaff.DevelopmentRating(high,s.UserTeam,Position.QB),35,99);
        Assert.InRange(ProStaff.DevelopmentRating(low,s.UserTeam,Position.QB),35,99);
    }

    [Fact] public void FiveYearHighAndLowDevelopmentRemainBelowStarInflationAndAboveCollapse()
    {
        ProPlayer Player(int index)
        {
            var overall=78;var bytes=new byte[16];BitConverter.GetBytes(index+1).CopyTo(bytes,0);
            return ProPlayerRatingService.Refresh(new(new DynastyPlayer{PlayerId=new Guid(bytes),FullName="Balance",TeamName="Team",
                Position=(Position)(index%Enum.GetValues<Position>().Length),OverallRating=overall,PotentialRating=90,SeasonStartOverall=overall,
                SpeedRating=overall,StrengthRating=overall,AgilityRating=overall,AwarenessRating=overall,InjuryRating=85,
                Skills=new(){PassingPower=overall,PassingAccuracy=overall,Catching=overall,BreakTackle=overall,Tackling=overall,
                    RunBlocking=overall,PassBlocking=overall,BallCarrying=overall,Acceleration=overall,Leadership=overall,
                    Discipline=overall,Jumping=overall,KickPower=overall,KickAccuracy=overall}},22,0,false),resetBaseline:true);
        }
        ProPlayer[] Grow(int development)
        {
            var players=Enumerable.Range(0,300).Select(Player).ToArray();var random=new Random(development);
            for(var year=0;year<5;year++)players=players.Select(p=>ProDevelopment.AgeOneYear(p,random,development)).ToArray();
            return players;
        }
        var high=Grow(95);var low=Grow(40);
        Assert.All(high.Concat(low),p=>Assert.InRange(p.Football.OverallRating,40,99));
        Assert.All(high.Concat(low),p=>Assert.True(p.Football.OverallRating<=p.Football.PotentialRating));
        Assert.InRange(low.Average(p=>p.Football.OverallRating),72,84);
        Assert.InRange(high.Average(p=>p.Football.OverallRating),76,88);
        Assert.True(high.Count(p=>p.Football.OverallRating>=90)<high.Length*.20,$"avg={high.Average(p=>p.Football.OverallRating):F1} elite={high.Count(p=>p.Football.OverallRating>=90)}/{high.Length}");
    }

    [Fact] public void AnnualProLifecycleAgesStaffRetiresAndGeneratesReplacementCandidates()
    {
        var s=New();
        var completed=s with{Games=s.Games.Select(game=>game.Phase==ProPhase.RegularSeason?
            game with{Football=game.Football with{HasPlayed=true,HomeScore=24,AwayScore=17}}:game).ToList()};
        var cpu=s.UserTeam=="Seattle Seahawks"?s.League.Teams.Single(t=>t.Id=="SF"):s.League.Teams.Single(t=>t.Name==s.UserTeam);
        var cpuHead=completed.Coaches.First(c=>c.TeamId==cpu.Id&&c.Role==StaffRole.HeadCoach);
        var userHead=completed.Coaches.First(c=>c.TeamId==s.League.Teams.Single(t=>t.Name==s.UserTeam).Id&&c.Role==s.Career!.Role);
        var medical=completed.MedicalStaff.First(c=>c.Team==s.UserTeam);
        var scout=completed.Scouts.First(c=>c.Team==s.UserTeam);
        var former=completed.Players.First(p=>p.Football.TeamName==s.UserTeam);
        completed=completed with{
            Coaches=completed.Coaches.Select(c=>c.TeamId==cpu.Id&&c.Role==StaffRole.HeadCoach?c with{Age=90}:
                c.TeamId==userHead.TeamId&&c.Role==userHead.Role?c with{Age=90}:c).ToList(),
            MedicalStaff=completed.MedicalStaff.Select(c=>c.Staff.StaffId==medical.Staff.StaffId?c with{Staff=c.Staff with{Age=90}}:c).ToList(),
            Scouts=completed.Scouts.Select(c=>c.Staff.ScoutId==scout.Staff.ScoutId?c with{Staff=c.Staff with{Age=90}}:c).ToList(),
            Players=completed.Players.Select(p=>p.Football.PlayerId==former.Football.PlayerId?p with{Retired=true,
                Football=p.Football with{TeamName="Retired",Skills=p.Football.Skills! with{Leadership=95}}}:p).ToList()};
        var oldAges=completed.Coaches.Where(c=>!c.Vacant).ToDictionary(c=>c.TeamId+c.Role,c=>c.Age);
        var next=ProStaff.DevelopSeason(completed);
        Assert.Equal(completed.Year,next.StaffLifecycleYear);
        Assert.DoesNotContain(next.Coaches,c=>c.TeamId==cpu.Id&&c.Role==StaffRole.HeadCoach&&!c.Vacant);
        Assert.Contains(next.Coaches,c=>c.TeamId==userHead.TeamId&&c.Role==userHead.Role&&!c.Vacant&&c.Age==91);
        Assert.Equal(64,next.MedicalStaff.Count);
        Assert.Equal(96,next.Scouts.Count);
        Assert.Contains(next.StaffRetirements,r=>r.Team==cpu.Name&&r.Department=="Coaching");
        Assert.Contains(next.StaffRetirements,r=>r.Team==s.UserTeam&&r.Department=="Medical");
        Assert.Contains(next.StaffRetirements,r=>r.Team==s.UserTeam&&r.Department=="Scouting");
        Assert.Contains(next.CoachCandidates,c=>c.Name==former.Football.FullName&&c.Source=="Retired player");
        next=ProStaff.ManageCpu(next);
        Assert.Contains(next.Coaches,c=>c.TeamId==cpu.Id&&c.Role==StaffRole.HeadCoach&&!c.Vacant);
    }

    [Fact] public void LegacyFranchiseReceivesPlansProfilesAndMedicalStaffWithoutChangingPlayers()
    {
        var original=New();
        var legacy=original with{GamePlans=[],MedicalStaff=[],Coaches=original.Coaches.Select(c=>c with{Coaching=null}).ToList()};
        var migrated=ProFootballSetup.Ensure(legacy);
        Assert.Equal(legacy.Players,migrated.Players);
        Assert.Equal(legacy.Scouts,migrated.Scouts);
        Assert.Equal(32,migrated.GamePlans.Count);
        Assert.Equal(64,migrated.MedicalStaff.Count);
        Assert.All(migrated.Coaches.Where(c=>!c.Vacant),c=>Assert.NotNull(c.Coaching));
        var repeated=ProFootballSetup.Ensure(migrated);
        Assert.Equal(migrated.GamePlans,repeated.GamePlans);
        Assert.Equivalent(migrated.Coaches,repeated.Coaches,strict:true);
        Assert.Equivalent(migrated.MedicalStaff,repeated.MedicalStaff,strict:true);
        Assert.Equivalent(migrated.Scouts,repeated.Scouts,strict:true);
    }

    [Fact] public async Task MedicalAndPotentialScoutingSurviveRoundtripAndLegacyReportsRemainReadable()
    {
        var s=ProDraft.Generate(New() with{Phase=ProPhase.Offseason,OffseasonStep=OffseasonStep.Draft});
        s=s with{Scouting=s.Scouting.Select(report=>report.PotentialHigh>0?report:report with{PotentialLow=60,PotentialHigh=72,PotentialConfidence=55}).ToList()};
        var dir=Path.Combine(Path.GetTempPath(),"staff-balance-"+Guid.NewGuid());
        try{var store=new ProSaveStore(dir);await store.Save(s);var loaded=await store.Load(s.Id);
            Assert.Equivalent(s.MedicalStaff,loaded.MedicalStaff,strict:true);Assert.Equivalent(s.Scouting,loaded.Scouting,strict:true);
            Assert.All(loaded.Coaches.Where(c=>!c.Vacant),c=>Assert.NotNull(c.Coaching));}
        finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
    }
}
