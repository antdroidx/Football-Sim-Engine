using DynastyManager.Core.Models;

namespace DynastyManager.App;

public sealed record GameFieldStyle(Color Turf, Color TurfAccent, Color Line, Color Text, Color Home, Color Away, string HomeInitial, string HomeName)
{
    public static GameFieldStyle From(GameDaySession session, GameCastLiveBrand home, GameCastLiveBrand away)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(home);
        ArgumentNullException.ThrowIfNull(away);
        var homeColor = Color.FromArgb(home.Primary);
        var awayColor = Color.FromArgb(away.Primary);
        var turf = session.Weather.Indoors ? Color.FromArgb("#24563C") : Color.FromArgb("#245B38");
        var turfAccent = session.Weather.Indoors ? Color.FromArgb("#2B6045") : Color.FromArgb("#285D3D");
        var line = Color.FromArgb("#D5E6DB");
        var text = session.Weather.Indoors ? Color.FromArgb("#EAF2EC") : Color.FromArgb("#D5E6DB");
        return new(turf, turfAccent, line, text, homeColor, awayColor, home.Abbreviation, home.FieldName ?? home.Abbreviation);
    }
}
