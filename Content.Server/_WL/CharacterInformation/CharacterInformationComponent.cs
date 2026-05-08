namespace Content.Server._WL.CharacterInformation;

/// <summary>
///     Adds examine details verb and store information that can be accessed without mob actor
/// </summary>
[RegisterComponent]
public sealed partial class CharacterInformationComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public string FlavorText = string.Empty;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public string OocText = string.Empty;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public string DynamicText = string.Empty;
}
