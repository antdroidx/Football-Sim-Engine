using System.Text.Json;
using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using ProFootball.Core;

static class DeviceAcceptanceFixture
{
    public static async Task Run(string output)
    {
        var state=ProLeagueService.Create("Seattle Seahawks") with {Name="Safety Acceptance"};
        var game=state.Games.First(g=>g.Phase==state.Phase&&g.Football.Week==state.Week&&g.Football.InvolvesTeam(state.UserTeam));
        GameDaySession? found=null;
        for(var seed=1;seed<=10000 && found is null;seed++)
        {
            var candidate=game with {Football=game.Football with {SimulationSeed=seed}};
            var s=ProLeagueService.Start(state,candidate,GameDayMode.Watch);
            while(!s.IsComplete&&s.Events.Count<25)
            {
                var e=GameDayEngine.Step(s);
                if(e.IsSafety){found=s;game=candidate;break;}
            }
        }
        if(found is null) throw new Exception("No early safety seed found");
        state=state with {Games=state.Games.Select(g=>g.Football.GameId==game.Football.GameId?game:g).ToList()};
        var safety=found.Events.Last();
        var kick=GameDayEngine.Step(found);
        if(!kick.IsSafetyKick || kick.StartYardLine!=80)throw new Exception("Invalid free kick");
        var store=new ProSaveStore(output);
        await store.Save(state);
        var loaded=await store.Load(state.Id);
        if(loaded.Games.Single(g=>g.Football.GameId==game.Football.GameId).Football.SimulationSeed!=game.Football.SimulationSeed)throw new Exception("Seed did not reload");
        var replay=ProLeagueService.Start(loaded,loaded.Games.Single(g=>g.Football.GameId==game.Football.GameId),GameDayMode.Watch);
        while(replay.Events.Count<safety.Sequence) GameDayEngine.Step(replay);
        if(!replay.Events[^1].IsSafety || replay.Events[^1].ScoringTeamName!=safety.ScoringTeamName) throw new Exception("Loaded safety sequence changed");
        GameDayEngine.SimulateToEnd(found);
        var completed=ProLeagueService.Finish(state,found) with {Id=Guid.NewGuid(),Name="Safety Box Acceptance"};
        await store.Save(completed);await store.Load(completed.Id);
        var colors=state with {Id=Guid.NewGuid(),Name="Staff Palette Acceptance",Coaches=state.Coaches.Select(c=>
            ProStaff.Apply(c,CoachingAttributes.FromValues([0,49,50,59,60,79,89,100]))).ToList()};
        await store.Save(colors);await store.Load(colors.Id);
        File.WriteAllText(Path.Combine(output,"manifest.json"),JsonSerializer.Serialize(new {LiveId=state.Id,BoxId=completed.Id,ColorsId=colors.Id,game.Football.GameId,game.Football.SimulationSeed,SafetySequence=safety.Sequence,SafetyTeam=safety.ScoringTeamName,SafetyClock=safety.ClockSeconds,FreeKickSequence=kick.Sequence},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Production-loaded fixtures written: safety sequence {safety.Sequence}, seed {game.Football.SimulationSeed}");
    }
}
