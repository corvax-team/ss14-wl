using Content.Shared.Charges.Components;

namespace Content.Server._WL.MetaData.Components;

/// <summary>
/// Component that allows an entity to be renamed through interaction.
/// </summary>
[RegisterComponent]
public sealed partial class RenameOnInteractComponent : Component
{
    /// <summary>
    /// Whether renaming this entity requires charges (from <see cref="LimitedChargesComponent"/>).
    /// </summary>
    [DataField]
    public bool NeedCharges { get; set; } = true;

    /// <summary>
    /// Whether to expose the rename action as an interaction verb.
    /// </summary>
    [DataField]
    public bool UseVerbs { get; set; } = true;
}
