using Content.Shared.Preferences;

namespace Content.Shared._WL.Passports.Events;

public sealed class SpawnPassportEvent : EntityEventArgs;

[ByRefEvent]
public sealed class PassportToggleEvent : HandledEntityEventArgs;

[ByRefEvent]
public sealed class PassportProfileUpdatedEvent(HumanoidCharacterProfile profile) : HandledEntityEventArgs
{
    public HumanoidCharacterProfile Profile { get; } = profile;
}
