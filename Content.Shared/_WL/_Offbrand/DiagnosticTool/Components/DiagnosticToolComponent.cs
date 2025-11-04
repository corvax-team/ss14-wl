using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL._Offbrand.DiagnosticTool.Components;

/// <summary>
///     Adds a verb and action that allows the user to listen to the entity's breathing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DiagnosticToolComponent: Component
{
    /// <summary>
    ///     Time between each use of the stethoscope.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1.75);

    [DataField]
    public bool Cyclable = false;

    [DataField]
    public string Verb = "penlight-verb";

    [DataField]
    public string[] LevelsDesc = ["penlight-0", "penlight-1", "penlight-2", "penlight-3"];

    [DataField]
    public string[] DeltaVerbs = ["penlight-worse", "penlight-stable", "penlight-better"];

    [DataField]
    public string NothingVerb = "penlight-nothing";

    /// <summary>
    ///     Last damage that was measured. Used to indicate if breathing is improving or getting worse.
    /// </summary>
    [DataField]
    public FixedPoint2? LastMeasuredDamage;

    [DataField]
    public EntProtoId Action = "ActionPenlight";

    [DataField]
    public EntityUid? ActionEntity;
}
