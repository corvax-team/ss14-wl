using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics.Visuals
{
    [Serializable, NetSerializable]
    public enum PokerCardState : byte
    {
        IsFlipped
    }

    [Serializable, NetSerializable]
    public enum PokerCardLayers : byte
    {
        Flipped,
        NonFlipped
    }
}
