namespace DynastyManager.Core.Models;

public sealed record ScoutAttributes
{
    public int Evaluation { get; init; } = 65;
    public int Communication { get; init; } = 65;
    public int RegionalKnowledge { get; init; } = 65;
    public int NationalKnowledge { get; init; } = 65;
    public (string Name,int Rating)[] Values() => [("Evaluation",Evaluation),("Communication",Communication),("Regional Knowledge",RegionalKnowledge),("National Knowledge",NationalKnowledge)];
    public static ScoutAttributes FromValues(IReadOnlyList<int> v) => new()
        {Evaluation=Math.Clamp(v[0],0,100),Communication=Math.Clamp(v[1],0,100),RegionalKnowledge=Math.Clamp(v[2],0,100),NationalKnowledge=Math.Clamp(v[3],0,100)};
    public ScoutAttributes Improve(int change) => FromValues(Values().Select(v=>v.Rating+change).ToArray());
    public int PredictionQuality(bool regional) => (int)Math.Round(Evaluation*.5+Communication*.2+(regional?RegionalKnowledge:NationalKnowledge)*.3);
    public static ScoutAttributes FromLegacy(ScoutStaff scout) => scout.Attributes ?? new()
        {Evaluation=scout.TalentEvaluation,Communication=scout.WorkRate,RegionalKnowledge=scout.RegionalKnowledge,NationalKnowledge=scout.PotentialEvaluation};
    public static ScoutAttributes FromLegacy(StaffMember chief) => chief.ScoutingRatings ?? new()
        {Evaluation=chief.TalentEvaluation,Communication=chief.StaffManagement,RegionalKnowledge=chief.RegionalKnowledge,NationalKnowledge=chief.PotentialEvaluation};
}
