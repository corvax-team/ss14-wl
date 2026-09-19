using Content.Shared.Research.Prototypes;
using Content.Shared.Research.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Research.Components;

/// <summary>
/// This is used for an item that is inserted directly into a given lathe to provide it with a recipe.
/// </summary>
[RegisterComponent, NetworkedComponent] // WL-Changes: Birdscience Removed Access(typeof(BlueprintSystem)). Stop add access without no fucking reason.
public sealed partial class BlueprintComponent : Component
{
    /// <summary>
    /// The recipes that this blueprint provides.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<LatheRecipePrototype>> ProvidedRecipes = new();
}
