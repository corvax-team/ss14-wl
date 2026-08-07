namespace Content.Shared._WL.Administration;

[RegisterComponent]
public sealed partial class AdminObjectiveComponent : Component
{
    [DataField]
    public float Progress;

    [DataField]
    public int TaskId;
}
