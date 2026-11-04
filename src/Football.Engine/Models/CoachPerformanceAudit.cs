namespace DynastyManager.Core.Models;

public sealed record CoachPerformanceAudit(
    Guid StaffId,
    string FullName,
    string TeamName,
    StaffRole Role,
    int SeasonYear,
    int Prestige,
    int Wins,
    int Losses,
    int ExpectedWins,
    int WinDelta,
    int PerformanceChange,
    string Summary);
