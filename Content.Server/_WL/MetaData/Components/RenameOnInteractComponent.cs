using Content.Shared.Charges.Components;
using Robust.Shared.Utility;

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

    [DataField]
    public LocId RenameActionLocString = "renameable-component-rename-action";

    [DataField]
    public LocId NameTitleLocString = "renameable-component-name-field";

    [DataField]
    public LocId NewNameConditions = "renameable-system-new-name-conditions";
    [DataField]
    public ResPath VerbTexturePath = new("/Textures/Interface/AdminActions/rename.png");
}
