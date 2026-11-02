using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;
namespace DynastyManager.Core.Tests;
[Trait("Owner", "College")]
[Trait("Category", "Fast")]
public sealed class CollegeStatGamesTests
{
    [Fact]
    public void ConferenceTitleAndBowlStayDistinctFromRegularSeason()
    {
        ScheduledGame Game(ScheduledGameType type) => new() {GameId=type.ToString(),HomeTeamName="A",AwayTeamName="B",GameType=type};
        var tagged=CollegeStatGames.Tag(Enum.GetValues<ScheduledGameType>().Select(Game)).ToArray();
        Assert.Equal(2,tagged.Count(g=>g.Competition==StatCompetition.RegularSeason));
        Assert.Equal(2,tagged.Count(g=>g.Competition==StatCompetition.Postseason));
        Assert.Equal(ScheduledGameType.ConferenceChampionship,Assert.Single(tagged.Where(g=>g.Competition==StatCompetition.ConferenceChampionship)).Game.GameType);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void KickoffPreviewAndEventUseTheSameSpecialistsAndElevenPlayers(bool onside)
    {
        var s=GameDayEngineTests.Session(GameDayMode.Watch,includeOpeningKickoff:true);
        s.OnsideKickRequested=onside;
        var receiving=GameDayEngine.KickoffUnit(s,true).Select(p=>p.PlayerId).ToArray();
        var kicking=GameDayEngine.KickoffUnit(s,false).Select(p=>p.PlayerId).ToArray();
        var e=GameDayEngine.Step(s);
        Assert.Equal(receiving,e.OffensePersonnel);Assert.Equal(kicking,e.DefensePersonnel);
        Assert.Equal(11,receiving.Distinct().Count());Assert.Equal(11,kicking.Distinct().Count());
        Assert.Contains(e.KickerId!.Value,kicking);
    }
}
