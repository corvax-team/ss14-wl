using Robust.Shared.Serialization;

namespace Content.Shared._WL.SafeCode;


[Serializable, NetSerializable]
public sealed class SafeCodeBoundUserInterfaceState(int codeLength, bool locked, bool? lastAttemptCorrect = null) : BoundUserInterfaceState
{
    public int CodeLength { get; } = codeLength;
    public bool Locked { get; } = locked;
    public bool? LastAttemptCorrect { get; } = lastAttemptCorrect;
}


[Serializable, NetSerializable]
public sealed class SafeCodeRequestMessage(string code) : BoundUserInterfaceMessage
{
    public string Code { get; } = code;
}

[Serializable, NetSerializable]
public sealed class SafeCodeLockRequestMessage : BoundUserInterfaceMessage;
