using Robust.Shared.Serialization;

namespace Content.Shared._WL.Photo;

[Serializable, NetSerializable]
public sealed class PhotoCardUiState(byte[]? imageData) : BoundUserInterfaceState
{
    public byte[]? ImageData { get; } = imageData;
}

[Serializable, NetSerializable]
public enum PhotoCardUiKey : byte
{
    Key
}
