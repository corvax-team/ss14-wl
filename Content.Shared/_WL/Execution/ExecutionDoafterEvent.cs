using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Execution;

[Serializable, NetSerializable]
public sealed partial class ExecutionDoAfterEvent : SimpleDoAfterEvent
{
}
