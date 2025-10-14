using Robust.Shared.Serialization;

namespace Content.Shared._WL.DynamicText;

[Serializable, NetSerializable]
public sealed class DynamicTextEvent(NetEntity netEntity, string dynamicText) : EntityEventArgs
{
    public NetEntity Entity { get; } = netEntity;
    public string DynamicText { get; } = dynamicText;
}
public sealed class RequestDynamicTextEvent(string dynamicText) : EntityEventArgs
{
    public string DynamicText { get; } = dynamicText;
}
