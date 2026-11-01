using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.DisplacementMap;
using System.Numerics;

namespace Content.Shared._WL.Implants;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ImitatorDisguiseComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Image;

    [DataField, AutoNetworkedField]
    public float Instability;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ImitatorImageComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    [DataField]
    public Vector2 Scale = Vector2.One;

    [DataField, AutoNetworkedField]
    public string? ClothingSpecies;

    [DataField, AutoNetworkedField]
    public Dictionary<string, DisplacementData> Displacements = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, DisplacementData> MaleDisplacements = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, DisplacementData> FemaleDisplacements = new();
}

public sealed partial class ImitatorCopyActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class ImitatorCopyDoAfterEvent : SimpleDoAfterEvent;
