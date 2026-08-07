using Robust.Shared.Serialization;

namespace Content.Shared.Corvax.TTS;

[Serializable, NetSerializable]
// ReSharper disable once InconsistentNaming
public sealed class PlayTTSEvent : EntityEventArgs
{
    public byte[] Data { get; }
    public NetEntity? SourceUid { get; }
    public bool IsWhisper { get; }
    // WL-Changes-Start: TTS preview playback
    public bool IsPreview { get; }

    public PlayTTSEvent(
        byte[] data,
        NetEntity? sourceUid = null,
        bool isWhisper = false,
        bool isPreview = false)
    {
        Data = data;
        SourceUid = sourceUid;
        IsWhisper = isWhisper;
        IsPreview = isPreview;
    }
    // WL-Changes-End
}
