using Content.Shared.MassMedia.Systems;

namespace Content.Shared.MassMedia.Components;

[RegisterComponent]
public sealed partial class StationNewsComponent : Component
{
    [DataField]
    public List<NewsArticle> Articles = new();

    [DataField]
    public List<NewsArticle> DeletedArticles = new(); // WL-Changes: send deleted news
}
