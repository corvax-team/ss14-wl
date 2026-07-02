using Robust.Shared.Serialization;

namespace Content.Shared._WL.SafeCode;

[Serializable, NetSerializable]
public enum SafeCodeVisuals : byte
{
    Locked,
    Broken
}

[Serializable, NetSerializable]
public enum SafeCodeVisualLayers : byte
{
    Base,
    Lights
}
