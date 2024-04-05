using Robust.Shared.Serialization;

namespace Content.Shared._WL.PulseDemon;

[Serializable, NetSerializable]
public enum PulseDemonState : byte
{
    IsHiding
}

[Serializable, NetSerializable]
public enum PulseDemonVisualLayers : byte
{
    Demon
}
