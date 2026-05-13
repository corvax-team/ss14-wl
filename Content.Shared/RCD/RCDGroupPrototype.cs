// WL-Changes-start: dehardcode
using Robust.Shared.Prototypes;

namespace Content.Shared.RCD;

[Prototype("rcdGroup")]
public sealed partial class RCDGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = "";

    [DataField]
    public string Sprite = "";
}
// WL-Changes-end
