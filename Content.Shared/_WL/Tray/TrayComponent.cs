using Content.Shared.Hands.Components;
using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Tray;

[RegisterComponent]
public sealed partial class TrayComponent : Component
{
    [DataField]
    public int Capacity = 3;

    [DataField]
    public ProtoId<ItemSizePrototype> MaxItemSize = "Small";

    [ViewVariables]
    public Dictionary<EntityUid, EntityCoordinates> ConnectedEntities = new();

    [ViewVariables]
    public ProtoId<ItemSizePrototype> DefaultSize = "Small";
    [DataField]
    public ProtoId<ItemSizePrototype> FilledSize = "Ginormous";

    [DataField]
    public bool HasCap = false;
    [ViewVariables]
    public bool Closed = false;
    [DataField]
    public SoundSpecifier CloseSound = new SoundPathSpecifier("/Audio/_WL/Effects/metal_pot_cover.ogg");
    [DataField]
    public SoundSpecifier OpenSound = new SoundPathSpecifier("/Audio/_WL/Effects/metal_pot_open.ogg");

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> ItemsInhandVisuals = new();
    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> CapInhandVisuals = new();
}

[Serializable, NetSerializable]
public enum TrayVisualLayers : byte
{
    Items,
    Cap
}

[Serializable, NetSerializable]
public enum TrayVisualState : byte
{
    HasEntities,
    Closed
}
