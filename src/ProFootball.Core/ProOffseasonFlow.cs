using DynastyManager.Core.Models;
namespace ProFootball.Core;
public static class ProOffseasonFlow
{
    public static string Title(OffseasonStep step)=>step switch{OffseasonStep.Retention=>"Contract retention",OffseasonStep.FreeAgency=>"Free agency",OffseasonStep.Draft=>"Draft",_=>"Training camp"};
    public static string Continue(OffseasonStep step)=>step switch{OffseasonStep.Retention=>"Finish retention & open free agency",OffseasonStep.FreeAgency=>"Complete free agency & open draft",OffseasonStep.Draft=>"Continue to training camp",_=>"Begin preseason"};
    public static string? Blocker(ProFranchise s)=>Validate(s).FirstReason;
    public static PhaseValidation Validate(ProFranchise s)=>s.OffseasonStep switch{
        OffseasonStep.Draft when ProDraft.Current(s) is not null=>PhaseValidation.Blocked("draft-incomplete","Complete the draft, or delegate your remaining selections.","Draft"),
        OffseasonStep.Draft when !s.AutoManageRookieOptions&&ProRookieOptions.Pending(s,s.UserTeam).Any(p=>ProCareer.Controls(s,p.Football.Position))=>PhaseValidation.Blocked("rookie-options-pending","Decide the fifth-year options due before leaving the draft.","RookieOptions"),
        OffseasonStep.RosterCuts when ProRoster.CountForLimit(s,s.UserTeam)>90=>PhaseValidation.Blocked("camp-roster-limit","Reduce your training-camp roster to 90 players.","Camp"),_=>PhaseValidation.Ready};
}
