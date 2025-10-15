using Robust.Shared.Serialization;

namespace Content.Shared._WL.DynamicText;

[Serializable, NetSerializable]
public sealed class RequestDynamicTextEvent(string dynamicText) : EntityEventArgs
{
    public string DynamicText { get; } = dynamicText;
}
