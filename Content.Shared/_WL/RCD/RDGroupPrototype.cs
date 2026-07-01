using Robust.Shared.Prototypes;

namespace Content.Shared.RCD;

[Prototype("rdGroup")]
public sealed partial class RDGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = "";

    [DataField]
    public string Sprite = "";
}
