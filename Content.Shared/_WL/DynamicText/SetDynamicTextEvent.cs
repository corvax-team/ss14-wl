using Robust.Shared.Serialization;

namespace Content.Shared._WL.DynamicText;

[Serializable, NetSerializable]
public sealed class SetDynamicTextEvent(NetEntity netEntity, string dynamicText) : EntityEventArgs
{
    public NetEntity Entity { get; } = netEntity;
    public string DynamicText { get; } = dynamicText;
}
