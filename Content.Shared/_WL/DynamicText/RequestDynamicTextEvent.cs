using Robust.Shared.Serialization;

namespace Content.Shared._WL.DynamicText;

[Serializable, NetSerializable]
public sealed class RequestDynamicTextEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity { get; } = entity;
}
