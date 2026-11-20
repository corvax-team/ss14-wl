using Robust.Shared.Serialization;

namespace Content.Shared._WL.CombatIndicator;

[Serializable, NetSerializable]
public sealed class SetCombatIndicatorAutoEnableEvent(bool enabled) : EntityEventArgs
{
    public bool Enabled { get; } = enabled;
}
