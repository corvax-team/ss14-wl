using Content.Shared.StationRecords;
using Robust.Shared.Audio; // WL-Records
using Robust.Shared.Prototypes; // WL-Records
using Robust.Shared.GameStates;

namespace Content.Shared.StationRecords.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneralStationRecordConsoleComponent : Component
{
    /// <summary>
    /// Selected crewmember record id.
    /// Station always uses the station that owns the console.
    /// </summary>
    [DataField]
    public uint? ActiveKey;

    /// <summary>
    /// Qualities to filter a search by.
    /// </summary>
    [DataField]
    public StationRecordsFilter? Filter;

    /// <summary>
    /// Whether this Records Console is able to delete entries.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanDeleteEntries;

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
    // WL-Record-end
}
