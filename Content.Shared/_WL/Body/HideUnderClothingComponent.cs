using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.Body;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class HideUnderClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Hidden;

    [DataField, AutoNetworkedField]
    public bool EarsHidden;

    [DataField, AutoNetworkedField]
    public bool TailHidden;

    [DataField, AutoNetworkedField]
    public EntityUid? Action;
}

public sealed partial class ToggleMarkingsUnderClothingEvent : InstantActionEvent;

[ByRefEvent]
public readonly record struct OrganMarkingsChangedEvent;

[ByRefEvent]
public readonly record struct MarkingsUnderClothingChangedEvent;
