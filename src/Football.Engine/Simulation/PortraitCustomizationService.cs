using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class PortraitCustomizationService
{
    public static DynastyState Apply(DynastyState state, Guid id, NcaaPlayerAppearance appearance, bool staff)
    {
        if (appearance.FacialHair is < 0 or > 3 || appearance.HairColor is < 0 or > 5 || appearance.HairStyle is < 0 or > 14)
            throw new ArgumentException("Invalid portrait style.", nameof(appearance));
        if (staff)
        {
            var target = state.Staff.FirstOrDefault(c => c.StaffId == id)
                ?? (state.UserHeadCoach?.StaffId == id ? state.UserHeadCoach : null);
            if (target is null || !target.TeamName.Equals(state.UserTeamName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only your program's staff portraits can be edited.");
            return state with
            {
                UserHeadCoach = state.UserHeadCoach?.StaffId == id ? state.UserHeadCoach with { Appearance = appearance } : state.UserHeadCoach,
                Staff = state.Staff.Select(coach => coach.StaffId == id ? coach with { Appearance = appearance } : coach).ToArray()
            };
        }
        return state with
        {
            ActiveRoster = state.ActiveRoster.Select(p => p.PlayerId == id ? p with { Appearance = appearance } : p).ToArray(),
            TransferPortalEntries = state.TransferPortalEntries.Select(p => p.Player.PlayerId == id
                ? p with { Player = p.Player with { Appearance = appearance } } : p).ToArray(),
            PendingTransferCommitments = state.PendingTransferCommitments.Select(p => p.Player.PlayerId == id
                ? p with { Player = p.Player with { Appearance = appearance } } : p).ToArray(),
            PlayerCareerHistory = state.PlayerCareerHistory.Select(p => p.PlayerId == id ? p with { Appearance = appearance } : p).ToArray(),
            PlayerSeasonStats = state.PlayerSeasonStats.Select(p => p.PlayerId == id ? p with { Appearance = appearance } : p).ToArray()
        };
    }
}
