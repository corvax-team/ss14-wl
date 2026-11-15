using Content.Shared.StationRecords;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Compromise;

[RegisterComponent]
public sealed partial class CompromiseTabletComponent : Component
{
    [DataField]
    public EntityUid? Station;

    [DataField]
    public uint? ActiveKey;

    [DataField]
    public StationRecordsFilter? Filter;

    [DataField]
    public uint MaxStringLength = 256;

    [DataField]
    public bool CanPrint = true;

    [DataField]
    public float TimePrint = 2.3f;

    [DataField]
    public float TimePrintRemaining;

    [DataField]
    public SoundSpecifier PrintAudio = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    [DataField]
    public EntProtoId PrintPaperId = "Paper";

    [DataField]
    public string ContextPrint = string.Empty;
}
