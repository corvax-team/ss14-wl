namespace Content.Shared._WL.Slimes.Components;

/*
 * It has been placed in a separate component, 
 * Because it is possible that later cores will be added for humanoid slimes.
 */
[RegisterComponent]
public sealed partial class SlimeCoreComponent : Component
{
    [DataField("researchPoints")]
    public int ResearchPoints;
}
