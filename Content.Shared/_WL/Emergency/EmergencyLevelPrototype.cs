using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._WL.Emergency;

[Prototype]
public sealed partial class EmergencyLevelPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<EmergencyLevelPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    [DataField(required: true)]
    private LocId Name { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField]
    public LocId? Announcement;

    [DataField]
    public LocId? UniqueStartAnnouncement;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public Color Color { get; private set; } = Color.Red;
}
