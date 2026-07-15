using Robust.Shared.Serialization;

namespace Content.Shared._WL.Stacks;

[Serializable, NetSerializable]
public sealed class StackAmountSetValueMessage(int value) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The new transfer amount.
    /// </summary>
    public int Value = value;
}

[Serializable, NetSerializable]
public enum StackAmountUiKey
{
    Key,
}

