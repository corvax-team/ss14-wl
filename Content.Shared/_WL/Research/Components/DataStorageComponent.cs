using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class DataStorageComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public List<ProtoId<StorageFormatPrototype>> AllowedStorageFormats;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<StorageFormatPrototype> CurrentStorageFormat;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Name;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Size;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 LocalSize;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 ExpiredSize = 0;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool CanBeFormatted = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool WriteAllowed = true;
}

[ByRefEvent]
public record struct RecalcExpiredSizeEvent(FixedPoint2 ExpiredSize);

[ByRefEvent]
public record struct DataUpdatedEvent();
