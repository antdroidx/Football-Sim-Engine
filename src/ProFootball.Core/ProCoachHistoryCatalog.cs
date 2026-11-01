using DynastyManager.Core.Models;
namespace ProFootball.Core;

public sealed record ProTeamPerformanceHistory(
    string TeamId, double OffenseRating, double DefenseRating, double WinRating);

public sealed record ProCoachTeamHistory(
    string TeamId, int Games, int Wins, int Losses, double PointsForPerGame,
    double PointsAgainstPerGame, int FirstSeason, int LastSeason);

public sealed record ProCoachPerformanceHistory
{
    public ProCoachPerformanceHistory(
        string name, double offenseRating, double defenseRating,
        ProCoachTeamHistory firstTenure, params ProCoachTeamHistory[] additionalTenures)
    {
        Name = name;
        OffenseRating = offenseRating;
        DefenseRating = defenseRating;
        Tenures = [firstTenure, ..additionalTenures];
    }

    public string Name { get; }
    public double OffenseRating { get; }
    public double DefenseRating { get; }
    public IReadOnlyList<ProCoachTeamHistory> Tenures { get; }
    public int Games => Tenures.Sum(tenure => tenure.Games);
    public int Wins => Tenures.Sum(tenure => tenure.Wins);
    public int Losses => Tenures.Sum(tenure => tenure.Losses);
    public double PointsForPerGame => Games == 0 ? 0 :
        Math.Round(Tenures.Sum(tenure => tenure.PointsForPerGame * tenure.Games) / Games, 1);
    public double PointsAgainstPerGame => Games == 0 ? 0 :
        Math.Round(Tenures.Sum(tenure => tenure.PointsAgainstPerGame * tenure.Games) / Games, 1);
    public int FirstSeason => Tenures.Min(tenure => tenure.FirstSeason);
    public int LastSeason => Tenures.Max(tenure => tenure.LastSeason);
}

/// <summary>
/// Evidence basis for initial Pro coach quality. Team units use weighted 2023-2025 regular-season scoring
/// percentiles from nflverse games.csv. Head coaches use their own observed game record and retain the club
/// associated with each tenure. Coordinators use their current club's relevant unit history because the source
/// does not identify coordinators.
/// </summary>
public static class ProCoachHistoryCatalog
{
    public const string SourceUrl = "https://github.com/nflverse/nfldata/blob/master/data/games.csv";
    public const string SalehAttributionUrl = "https://www.nfl.com/news/jets-fire-robert-saleh-grateful-for-time-as-jets-coach-in-first-comments-since-firing";
    public const string ObservedThrough = "2025 regular season";
    static readonly Lazy<IReadOnlyDictionary<string, ProTeamPerformanceHistory>> TeamIndex =
        new(() => TeamRows!.ToDictionary(item => item.TeamId, StringComparer.OrdinalIgnoreCase));
    static readonly Lazy<IReadOnlyDictionary<string, ProCoachPerformanceHistory>> CoachIndex =
        new(() => CoachRows!.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase));

    public static ProTeamPerformanceHistory Team(string teamId)
    {
        var sourceId=teamId switch{ "LAR"=>"LA", _=>teamId };
        return TeamIndex.Value[sourceId];
    }
    public static bool TryCoach(string name, out ProCoachPerformanceHistory history) => CoachIndex.Value.TryGetValue(name, out history!);

    public static int Rating(ProCoach coach)
    {
        var team=Team(coach.TeamId);
        double rating = coach.Role switch
        {
            StaffRole.HeadCoach when TryCoach(coach.Name,out var observed) =>
                68 + .25*(observed.OffenseRating-50) + .30*(observed.DefenseRating-50) +
                .20*(WinScore(observed.Wins,observed.Games)-50),
            StaffRole.OffensiveCoordinator =>
                68 + .50*(team.OffenseRating-50) + .15*(team.DefenseRating-50) + .05*(team.WinRating-50),
            StaffRole.DefensiveCoordinator =>
                68 + .50*(team.DefenseRating-50) + .15*(team.OffenseRating-50) + .05*(team.WinRating-50),
            StaffRole.SpecialTeamsCoordinator =>
                68 + .10*(team.OffenseRating-50) + .10*(team.DefenseRating-50) + .35*(team.WinRating-50),
            _ => 68 + .30*(team.OffenseRating-50) + .30*(team.DefenseRating-50) + .15*(team.WinRating-50)
        };
        return Math.Clamp((int)Math.Round(rating,MidpointRounding.AwayFromZero),48,90);
    }

    public static string Basis(ProCoach coach) => coach.Role == StaffRole.HeadCoach && TryCoach(coach.Name,out var observed)
        ? "Observed " + string.Join("; ", observed.Tenures.Select(tenure =>
            $"{tenure.TeamId} {SeasonSpan(tenure)}: {tenure.Wins}–{tenure.Losses} in {tenure.Games}, " +
            $"{tenure.PointsForPerGame:0.0} PPG scored, {tenure.PointsAgainstPerGame:0.0} allowed"))
        : $"Club proxy 2023–2025 (coordinator not named in source): offense {Team(coach.TeamId).OffenseRating:0.0}, " +
          $"defense {Team(coach.TeamId).DefenseRating:0.0}; role-weighted estimate";

    static string SeasonSpan(ProCoachTeamHistory tenure) => tenure.FirstSeason == tenure.LastSeason
        ? tenure.FirstSeason.ToString()
        : $"{tenure.FirstSeason}–{tenure.LastSeason}";

    static double WinScore(int wins,int games)=>games==0?50:50+60*(wins/(double)games-.5);

    static readonly ProTeamPerformanceHistory[] TeamRows =
    [
        new("ARI", 43.4, 33.2, 30.6),
        new("ATL", 41.1, 43.2, 48.1),
        new("BAL", 68.0, 58.9, 59.7),
        new("BUF", 74.6, 62.4, 71.3),
        new("CAR", 30.1, 37.6, 35.7),
        new("CHI", 50.0, 44.4, 46.5),
        new("CIN", 60.6, 29.7, 41.1),
        new("CLE", 29.1, 47.3, 36.3),
        new("DAL", 62.6, 31.0, 46.9),
        new("DEN", 55.2, 66.8, 64.7),
        new("DET", 74.6, 47.7, 66.5),
        new("GB", 57.2, 63.9, 55.6),
        new("HOU", 53.3, 67.6, 67.4),
        new("IND", 60.5, 36.8, 47.7),
        new("JAX", 55.6, 51.4, 55.0),
        new("KC", 47.1, 73.0, 56.8),
        new("LA", 66.3, 55.0, 62.4),
        new("LAC", 48.1, 63.4, 57.7),
        new("LV", 25.2, 39.7, 28.5),
        new("MIA", 44.2, 44.4, 48.5),
        new("MIN", 43.0, 67.2, 58.7),
        new("NE", 50.6, 59.5, 54.1),
        new("NO", 37.4, 52.3, 40.3),
        new("NYG", 35.9, 34.5, 25.8),
        new("NYJ", 28.1, 35.7, 28.7),
        new("PHI", 56.8, 64.3, 66.5),
        new("PIT", 47.3, 58.5, 59.1),
        new("SEA", 62.0, 64.3, 63.4),
        new("SF", 63.0, 51.4, 60.3),
        new("TB", 54.5, 50.6, 53.7),
        new("TEN", 26.8, 31.2, 22.9),
        new("WAS", 48.1, 33.0, 39.7),
    ];
    static readonly ProCoachPerformanceHistory[] CoachRows =
    [
        new("Kevin Stefanski", 34.2, 46.5, new("CLE", 51, 19, 32, 18.3, 23.1, 2023, 2025)),
        new("Dave Canales", 33.5, 36.5, new("CAR", 34, 13, 21, 19.2, 26.9, 2024, 2025)),
        new("Ben Johnson", 64.5, 37.4, new("CHI", 17, 11, 6, 25.9, 24.4, 2025, 2025)),
        new("Zac Taylor", 60.0, 32.3, new("CIN", 51, 24, 27, 24.5, 25.7, 2023, 2025)),
        new("Brian Schottenheimer", 68.4, 20.0, new("DAL", 17, 7, 10, 27.7, 30.1, 2025, 2025)),
        new("Sean Payton", 54.2, 60.6, new("DEN", 51, 32, 19, 23.2, 20.3, 2023, 2025)),
        new("Dan Campbell", 74.8, 48.4, new("DET", 51, 36, 15, 29.5, 22.5, 2023, 2025)),
        new("Matt LaFleur", 58.7, 64.5, new("GB", 51, 29, 22, 24.2, 20.5, 2023, 2025)),
        new("DeMeco Ryans", 52.9, 64.5, new("HOU", 51, 32, 19, 22.6, 20.0, 2023, 2025)),
        new("Shane Steichen", 59.4, 34.8, new("IND", 51, 25, 26, 24.3, 24.6, 2023, 2025)),
        new("Liam Coen", 70.3, 66.5, new("JAX", 17, 13, 4, 27.9, 19.8, 2025, 2025)),
        new("Andy Reid", 49.0, 74.2, new("KC", 51, 32, 19, 21.9, 18.6, 2023, 2025)),
        new("Jim Harbaugh", 51.9, 72.3, new("LAC", 34, 22, 12, 22.6, 18.9, 2024, 2025)),
        new("Sean McVay", 63.2, 52.3, new("LA", 51, 32, 19, 25.3, 21.7, 2023, 2025)),
        new("Kevin O'Connell", 45.2, 65.8, new("MIN", 51, 30, 21, 22.0, 20.1, 2023, 2025)),
        new("Mike Vrabel", 53.9, 62.6,
            new("TEN", 17, 6, 11, 17.9, 21.6, 2023, 2023),
            new ProCoachTeamHistory("NE", 17, 14, 3, 28.8, 18.8, 2025, 2025)),
        new("Kellen Moore", 27.7, 51.0, new("NO", 17, 6, 11, 18.0, 22.5, 2025, 2025)),
        new("John Harbaugh", 70.3, 63.9, new("BAL", 51, 33, 18, 27.9, 20.4, 2023, 2025)),
        new("Aaron Glenn", 25.8, 21.9, new("NYJ", 17, 3, 14, 17.6, 29.6, 2025, 2025)),
        new("Nick Sirianni", 60.6, 58.1, new("PHI", 51, 36, 15, 25.0, 20.7, 2023, 2025)),
        new("Mike McCarthy", 60.6, 47.1, new("DAL", 34, 19, 15, 25.3, 23.0, 2023, 2024)),
        new("Mike Macdonald", 61.6, 69.4, new("SEA", 34, 24, 10, 25.2, 19.4, 2024, 2025)),
        new("Kyle Shanahan", 64.5, 52.9, new("SF", 51, 30, 21, 25.8, 21.7, 2023, 2025)),
        new("Todd Bowles", 54.8, 54.2, new("TB", 51, 27, 24, 24.1, 22.0, 2023, 2025)),
        // nflverse labels every 2024 Jets game with Saleh; the NFL transaction record cuts his
        // sample after five games (2-3). Unit ratings are normalized to his 22-game sample.
        new("Robert Saleh", 24.2, 63.6, new("NYJ", 22, 9, 13, 16.4, 20.0, 2023, 2024)),
        new("Dan Quinn", 55.8, 38.4, new("WAS", 34, 17, 17, 24.7, 24.8, 2024, 2025)),
    ];
}
