using Robust.Shared.Serialization;

namespace Content.Shared._WL.SafeCode;


[Serializable, NetSerializable]
public sealed class SafeCodeBoundUserInterfaceState : BoundUserInterfaceState
{
    public int CodeLength { get; }
    public bool Locked { get; }
    public bool? LastAttemptCorrect { get; }

    public SafeCodeBoundUserInterfaceState(int codeLength, bool locked, bool? lastAttemptCorrect = null)
    {
        CodeLength = codeLength;
        Locked = locked;
        LastAttemptCorrect = lastAttemptCorrect;
    }
}


[Serializable, NetSerializable]
public sealed class SafeCodeRequestMessage : BoundUserInterfaceMessage
{
    public string Code { get; }

    public SafeCodeRequestMessage(string code)
    {
        Code = code;
    }
}

[Serializable, NetSerializable]
public sealed class SafeCodeLockRequestMessage : BoundUserInterfaceMessage
{
}
