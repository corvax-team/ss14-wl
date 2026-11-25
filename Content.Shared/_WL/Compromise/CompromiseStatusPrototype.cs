using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Compromise;

[Prototype]
public sealed partial class CompromiseCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    private LocId Name = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField("default")]
    public ProtoId<CompromisePrototype> DefaultCompromise { get; private set; } = default!;

    [DataField]
    public int Order = 0;
}

[Prototype]
public sealed partial class CompromisePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    private LocId Name = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField]
    private LocId Description = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedDesc => Loc.GetString(Description);

    [DataField]
    public ProtoId<CompromiseCategoryPrototype> Category { get; private set; } = "Finance";

    [DataField]
    public Color Color { get; private set; } = Color.White ;

    [DataField]
    public ResPath? Icon;

    [DataField]
    public int Order = 0;
}
