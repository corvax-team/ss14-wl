using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchScannerComponent : Component
{
    [DataField("detectTypes", required: true)]
    public List<ProtoId<ResearchTypePrototype>> ResearchType;

    [DataField("extrimalType")]
    public ProtoId<ResearchPointsTypePrototype> ExtrimalPointsType = "Experimental";

    [DataField("pointsType")]
    public ProtoId<ResearchPointsTypePrototype>? DefaultPointsType = null;

    [DataField]
    public float ScanDoAfterDuration = 5;

    [DataField]
    public FixedPoint4 ResearchPercent = 0.01;

    [DataField]
    public float Range = 2f;

    [DataField]
    public SoundSpecifier? CompleteSound = new SoundPathSpecifier("/Audio/Items/beep.ogg");

    [DataField, AutoNetworkedField]
    public bool Active = true;

    [DataField]
    public EntityWhitelist? Whitelist;
}

[Serializable, NetSerializable]
public sealed partial class ResearchScannerDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public record struct ScannedResearchEvent(EntityUid Target);

[ByRefEvent]
public record struct PostScannedEvent(EntityUid Target, List<string> Output);

[Serializable, NetSerializable]
public enum ResearchScannerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ResearchScannerUserInterfaceState : BoundUserInterfaceState
{
    public FormattedMessage Message;

    public bool ClearOutput;

    public ResearchScannerUserInterfaceState(FormattedMessage message, bool clearOutput)
    {
        Message = message;
        ClearOutput = clearOutput;
    }
}
