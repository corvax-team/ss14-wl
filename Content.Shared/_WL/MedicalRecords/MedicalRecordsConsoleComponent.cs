using Content.Shared.StationRecords;
using Robust.Shared.Audio; // WL-Records
using Robust.Shared.Prototypes; // WL-Records

namespace Content.Shared._WL.MedicalRecords.Components;

[RegisterComponent]
public sealed partial class MedicalRecordsConsoleComponent : Component
{
    [DataField]
    public uint? ActiveKey;

    [DataField]
    public StationRecordsFilter? Filter;

    // WL-Records-start
    [DataField]
    public bool CanPrintEntries = true;

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
    // WL-Records-end
}
