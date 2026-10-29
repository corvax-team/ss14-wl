using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Prototypes;

[Prototype]
public sealed partial class ResearchTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    private LocId Name { get; set; }

    [DataField(required: true)]
    public FixedPoint2 MinValue;

    [DataField(required: true)]
    public FixedPoint2 MaxValue;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField(required: true)]
    public ResearchCondition Condition;

    [DataField(required: true)]
    public ScaleMethod Scale;

    [DataField]
    public int Size = 100;

    public FixedPoint2 GetScale(int x)
    {
        return Scale.GetModifier(x, Size);
    }
}
