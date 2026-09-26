using Content.Shared._WL.Research;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Prototypes;

[Prototype]
public sealed partial class ResearchCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    private LocId Name { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField(required: true)]
    public double MaxPoints;

    [DataField(required: true)]
    public List<ProtoId<ResearchTypePrototype>> ResearchTypes;

    [DataField]
    public ProtoId<ResearchPointsTypePrototype> ExtrimalPoints = "Experimental";

    [DataField]
    public ProtoId<ResearchPointsTypePrototype>? PointsType = null;
}
