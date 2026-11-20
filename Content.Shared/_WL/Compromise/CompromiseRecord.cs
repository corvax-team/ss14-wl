using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Compromise;

[Serializable, NetSerializable]
public sealed partial record CompromiseRecord : StationRecord
{
    [DataField]
    public bool Valid = true;

    [DataField]
    public string CompromiseText;

    [DataField]
    public List<ProtoId<CompromisePrototype>> CompromiseData = new();

    public CompromiseRecord(string compromiseText, List<ProtoId<CompromisePrototype>> compromiseData)
    {
        CompromiseText = compromiseText;
        CompromiseData = compromiseData;
        Valid = compromiseData.Count != 0 || compromiseText != string.Empty;
    }
}
