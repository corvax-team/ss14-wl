using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

public abstract partial class BaseDataStorageComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 LocalSize;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 ExpiredLocalSize = 0;
}
