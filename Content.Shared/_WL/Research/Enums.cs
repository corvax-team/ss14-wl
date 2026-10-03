using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research;

[Serializable, NetSerializable]
[Flags]
public enum DAnalyzeType : int
{
    None = 0,
    Analyze = 1 << 0,
    DAnalyze = 1 << 1,
    Destruct = 1 << 2,

    All = -1
}
