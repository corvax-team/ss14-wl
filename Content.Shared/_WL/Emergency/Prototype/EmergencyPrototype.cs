using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Emergency.Prototype;

[Prototype]
public sealed partial class EmergencyListPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;


    [DataField]
    public HashSet<ProtoId<EmergencyPrototype>> Emergencys = new();


    [DataField]
    public string DefaultEmergency { get; private set; } = default!;
}

[Prototype]
public sealed partial class EmergencyPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = "Unknown";

    [DataField]
    public string Announcement { get; private set; } = string.Empty;

    [DataField]
    public string UniqueStartAnnouncement { get; private set; } = string.Empty;

    [DataField]
    public SoundSpecifier Sound { get; private set; } = new SoundPathSpecifier("/Audio/Misc/delta_alt.ogg");

    [DataField("soundAnnouncment")]
    public bool IsSoundAnnouncment { get; private set; } = true;

    [DataField]
    public Color Color { get; private set; } = Color.Red;
}
