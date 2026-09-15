using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UniversalComputerComponent : Component
{
    [DataField]
    public LocId VerbText = "uc-verb-close-program";

    [DataField, AutoNetworkedField]
    public List<ProtoId<ProgramPrototype>> Programs = new();

    [DataField, AutoNetworkedField]
    public ProtoId<ProgramPrototype> MenuProgram = "MenuProgram";

    [DataField, AutoNetworkedField]
    public ProtoId<ProgramPrototype> CurrentProgram = "MenuProgram";

    [AutoNetworkedField]
    public bool InMenu = true;
}

[RegisterComponent]
public sealed partial class UniversalComputerVisualsComponent : Component;

[Serializable, NetSerializable]
public enum UCMenuUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum UniversalComputerVisuals : byte
{
    ProgramPrototype
}

[Serializable, NetSerializable]
public sealed class ProgramOpenMessage(string program) : BoundUserInterfaceMessage
{
    public string Program = program;
}

[Serializable, NetSerializable]
public sealed class UniversalComputerBoundInterfaceState(List<ProtoId<ProgramPrototype>> programs) : BoundUserInterfaceState
{
    public List<ProtoId<ProgramPrototype>> Programs = programs;
}
