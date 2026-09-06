using Content.Shared._WL.Research;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Prototypes;

[Prototype]
public sealed partial class StorageFormatPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    private LocId Name { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField(required: true)]
    public Color Color;

    [DataField(required: true)]
    public ComponentRegistry StoragesList;
}
