namespace DynastyManager.Core.Models;

/// <summary>The shared football-coach attributes. College recruiting is a separate league-specific rating.</summary>
public sealed record CoachingAttributes
{
    public int Leadership { get; init; } = 60;
    public int Development { get; init; } = 60;
    public int OffensePlaycalling { get; init; } = 60;
    public int OffenseDevelopment { get; init; } = 60;
    public int DefenseScheme { get; init; } = 60;
    public int DefenseDevelopment { get; init; } = 60;
    public int SpecialTeams { get; init; } = 60;
    public int TalentEvaluation { get; init; } = 60;
    public (string Name, int Rating)[] Values() => [
        ("Leadership",Leadership), ("Development",Development), ("Offense Playcalling",OffensePlaycalling),
        ("Offense Development",OffenseDevelopment), ("Defense Scheme",DefenseScheme),
        ("Defense Development",DefenseDevelopment), ("Special Teams",SpecialTeams), ("Talent Evaluation",TalentEvaluation)];
    public static CoachingAttributes FromValues(IReadOnlyList<int> v) => new()
    {
        Leadership=Clamp(v[0]),Development=Clamp(v[1]),OffensePlaycalling=Clamp(v[2]),OffenseDevelopment=Clamp(v[3]),
        DefenseScheme=Clamp(v[4]),DefenseDevelopment=Clamp(v[5]),SpecialTeams=Clamp(v[6]),TalentEvaluation=Clamp(v[7])
    };
    public CoachingAttributes Improve(int change) => FromValues(Values().Select(v=>v.Rating+change).ToArray());
    public int Overall(StaffRole role, int? recruiting = null)
    {
        int[] values = role switch
        {
            StaffRole.OffensiveCoordinator => [Leadership,Development,OffensePlaycalling,OffenseDevelopment,TalentEvaluation],
            StaffRole.DefensiveCoordinator => [Leadership,Development,DefenseScheme,DefenseDevelopment,TalentEvaluation],
            StaffRole.SpecialTeamsCoordinator => [Leadership,Development,SpecialTeams,TalentEvaluation],
            _ => Values().Select(v=>v.Rating).ToArray()
        };
        return (int)Math.Round((recruiting is { } r ? values.Append(r) : values).Average());
    }
    public int DevelopmentFor(Position position) => position is Position.K or Position.P ? Development
        : (Development + (position is Position.QB or Position.RB or Position.FB or Position.WR or Position.TE or Position.OL ? OffenseDevelopment : DefenseDevelopment)) / 2;
    static int Clamp(int rating) => Math.Clamp(rating,0,100);
}

/// <summary>Medical and training staff have their own abilities rather than football-coaching attributes.</summary>
public sealed record MedicalAttributes
{
    public int Medical { get; init; } = 60;
    public int Training { get; init; } = 60;
    public int Conditioning { get; init; } = 60;
    public int Prevention { get; init; } = 60;
    public int Nutrition { get; init; } = 60;
    public (string Name,int Rating)[] Values() => [("Medical",Medical),("Training",Training),("Conditioning",Conditioning),("Prevention",Prevention),("Nutrition",Nutrition)];
    public static MedicalAttributes FromValues(IReadOnlyList<int> v) => new()
        {Medical=Math.Clamp(v[0],0,100),Training=Math.Clamp(v[1],0,100),Conditioning=Math.Clamp(v[2],0,100),Prevention=Math.Clamp(v[3],0,100),Nutrition=Math.Clamp(v[4],0,100)};
    public MedicalAttributes Improve(int change) => FromValues(Values().Select(v=>v.Rating+change).ToArray());
    public int Overall() => (int)Math.Round(Values().Average(v=>v.Rating));
}
