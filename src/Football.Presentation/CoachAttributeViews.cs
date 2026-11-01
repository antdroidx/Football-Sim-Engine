using DynastyManager.Core.Models;
namespace DynastyManager.App;

/// <summary>One attribute order, spelling and presentation for College and Pro coach profiles/editors.</summary>
public static class CoachAttributeViews
{
    public static View Coaching(CoachingAttributes attributes,int? recruiting=null)
    {
        var values=attributes.Values().AsEnumerable();
        if(recruiting is {} rating)values=values.Append(("Recruiting",rating));
        return PlayerAttributeViews.Group("Coaching attributes","",values.ToArray(),Title);
    }
    public static View Scouting(ScoutAttributes attributes) => PlayerAttributeViews.Group("Scout attributes","",attributes.Values(),Title);
    public static View Medical(MedicalAttributes attributes) => PlayerAttributeViews.Group("Medical staff attributes","",attributes.Values(),Title);
    static View Title(string title,string subtitle) => new VerticalStackLayout {Spacing=3,Children={
        new Label{Text=title,FontSize=18,FontAttributes=FontAttributes.Bold,TextColor=DynastyTheme.Text}}};
}

public sealed class CoachAttributeEditor : VerticalStackLayout
{
    readonly Entry[] entries;
    readonly int[] original;
    readonly bool medical;
    public CoachAttributeEditor(CoachingAttributes attributes,int? recruiting=null) : this(
        recruiting is {} r?attributes.Values().Append(("Recruiting",r)).ToArray():attributes.Values(),false) {}
    public CoachAttributeEditor(MedicalAttributes attributes):this(attributes.Values(),true) {}
    public CoachAttributeEditor(ScoutAttributes attributes):this(attributes.Values(),false) {}
    public ScoutAttributes ReadScouting() => entries.Length==4?ScoutAttributes.FromValues(Read()):throw new InvalidOperationException("Scouts use four attributes.");
    CoachAttributeEditor((string Name,int Rating)[] values,bool medical)
    {
        this.medical=medical;original=values.Select(v=>v.Rating).ToArray();Spacing=8;
        entries=values.Select(v=>new Entry{Text=v.Rating.ToString(),Keyboard=Keyboard.Numeric,AutomationId="coach-attribute-"+v.Name.ToLowerInvariant().Replace(' ','-')}).ToArray();
        for(var i=0;i<values.Length;i++)Add(new VerticalStackLayout{Spacing=2,Children={new Label{Text=values[i].Name,FontSize=12,TextColor=DynastyTheme.Muted},entries[i]}});
    }
    int[] Read() => entries.Select((e,i)=>int.TryParse(e.Text,out var v)?Math.Clamp(v,0,100):original[i]).ToArray();
    public CoachingAttributes ReadCoaching() => !medical?CoachingAttributes.FromValues(Read()):throw new InvalidOperationException("Medical staff use medical attributes.");
    public MedicalAttributes ReadMedical() => medical?MedicalAttributes.FromValues(Read()):throw new InvalidOperationException("Football coaches use coaching attributes.");
    public int ReadRecruiting() => entries.Length==9?Read()[8]:throw new InvalidOperationException("Recruiting is College-only.");
}
