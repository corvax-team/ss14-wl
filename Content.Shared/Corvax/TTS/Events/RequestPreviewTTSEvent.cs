using Robust.Shared.Serialization;

namespace Content.Shared.Corvax.TTS;

// ReSharper disable once InconsistentNaming
[Serializable, NetSerializable]
public sealed class RequestPreviewTTSEvent(string voiceId, string previewText /*WL-PreviewTTSEdit*/) : EntityEventArgs
{
    public string VoiceId { get; } = voiceId;

    public string PreviewText { get; } = previewText; //WL-PreviewTTSEdit
}
