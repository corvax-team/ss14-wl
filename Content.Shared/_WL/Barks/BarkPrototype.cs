using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Barks;

[Prototype("speechBark")]
public sealed partial class BarkPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public bool RoundStart = true;

    [DataField]
    public LocId Name = "bark-name-human1";

    [DataField]
    public string Category = "Standard_barks";

    [DataField(required: true)]
    public SoundSpecifier Sound { get; private set; } = default!;
}
