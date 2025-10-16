using Robust.Shared.Serialization;

namespace Content.Shared._WL.DynamicText;

[Serializable, NetSerializable]
public sealed class RequestedDynamicTextEvent(string dynamicText) : EntityEventArgs
{
    public string DynamicText { get; } = dynamicText;
}
