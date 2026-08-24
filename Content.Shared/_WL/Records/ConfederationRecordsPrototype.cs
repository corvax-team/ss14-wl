using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Roles;

namespace Content.Shared._WL.Records;

[Prototype("confederation")]
public sealed partial class ConfederationRecordsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = "Unknown";

    [DataField]
    public string Description { get; private set; } = "Unknown";

    [DataField]
    public ResPath? Icon { get; private set; }

    [DataField]
    public int Order { get; private set; }

    [DataField(serverOnly: true)]
    public JobSpecial[] Special { get; private set; } = [];

    [DataField]
    public ProtoId<EntityPrototype> PassportPrototype { get; private set; } = new();
}
