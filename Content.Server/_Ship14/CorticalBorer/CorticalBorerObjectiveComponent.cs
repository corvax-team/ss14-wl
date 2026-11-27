namespace Content.Server._Ship14.CorticalBorer;

[RegisterComponent]
public sealed partial class CorticalBorerObjectiveComponent : Component
{
    public const int MinimumRequiredInfected = 2;

    [DataField]
    public int RequiredInfected = MinimumRequiredInfected;
}
