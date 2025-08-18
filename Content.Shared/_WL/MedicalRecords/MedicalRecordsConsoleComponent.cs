using Content.Shared.StationRecords;

namespace Content.Shared._WL.MedicalRecords.Components;

[RegisterComponent]
public sealed partial class MedicalRecordsConsoleComponent : Component
{
    [DataField]
    public uint? ActiveKey;

    [DataField]
    public StationRecordsFilter? Filter;
}