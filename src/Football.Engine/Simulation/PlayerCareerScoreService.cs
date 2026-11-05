using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>League-neutral production scoring for recorded player stat lines.</summary>
public static class PlayerCareerScoreService
{
    public static double Production(PlayerSeasonStatLine p) => p.Position switch
    {
        Position.QB => p.PassYards + p.PassingTouchdowns * 150 - p.InterceptionsThrown * 200 + p.RushYards * 3 + p.RushingTouchdowns * 150,
        Position.RB or Position.FB or Position.WR or Position.TE => p.RushYards * 2.65 + p.ReceivingYards * 2.5 + p.Receptions * 2 +
            (p.RushingTouchdowns + p.ReceivingTouchdowns + p.KickReturnTouchdowns + p.PuntReturnTouchdowns) * 150 +
            p.KickReturnYards + p.PuntReturnYards - (p.RushingFumbles + p.ReceivingFumbles) * 75,
        Position.DE or Position.DT or Position.OLB or Position.MLB or Position.CB or Position.FS or Position.SS =>
            p.Tackles * 25 + (p.Sacks + p.Interceptions + p.FumbleRecoveries) * 425 + p.PassesDefended * 25,
        Position.K => (p.FieldGoalsMade * 25 + p.ExtraPointsMade * 5) * (p.FieldGoalsAttempted == 0 ? 1 : p.FieldGoalsMade / (double)p.FieldGoalsAttempted) +
            p.PuntYards * .10 + p.PuntsInside20 * 25 + p.PuntsInside10 * 40,
        _ => 0 // OL candidacy uses honors; pancake/blocking production is not recorded.
    };
}
