using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Slimes.Prototypes;

[DataDefinition]
[Serializable, NetSerializable]
[Prototype("slimeCommand")]
public sealed partial class SlimeCommandPrototype : SlimeCommandData, IPrototype
{

}

/// <summary>
/// A class containing data about a specific slime command.
/// </summary>
[Serializable, NetSerializable]
[Virtual, DataDefinition]
public partial class SlimeCommandData
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("command", required: true, serverOnly: true)]
    public SlimeCommand Command;

    [DataField("minPoints")]
    public int MinRelationshipPoints = 0;

    [DataField("maxPoints")]
    public int MaxRelationshipPoints = 500;

    [DataField("keywords", required: true)]
    public List<string> KeyWords = new();
}
