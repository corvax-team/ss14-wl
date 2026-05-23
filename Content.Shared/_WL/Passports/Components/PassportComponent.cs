using Content.Shared.Preferences;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.Passports.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PassportComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsClosed;

    [DataField, AutoNetworkedField]
    public TimeSpan ToggleCooldownEnd;

    [ViewVariables]
    public HumanoidCharacterProfile? OwnerProfile;

    [DataField, AutoNetworkedField]
    public string DisplayName = string.Empty;

    [DataField, AutoNetworkedField]
    public string DisplaySpecies = string.Empty;

    [DataField, AutoNetworkedField]
    public string DisplayGender = string.Empty;

    [DataField, AutoNetworkedField]
    public string DisplayYearOfBirth = string.Empty;

    [DataField, AutoNetworkedField]
    public string DisplayHeight = string.Empty;

    [DataField, AutoNetworkedField]
    public string DisplayPID = string.Empty;
}
