using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Prototypes;

[Prototype]
public sealed partial class ResearchModePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    private LocId Name { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField]
    public Color Color { get; set; }

    [DataField]
    public double PackagesModifier = 1f;

    [DataField]
    public double PointsModifier = 1f;
}
