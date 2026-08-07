using Content.Shared.Chat;
using Content.Shared._WL.CCVars; // WL-Changes
using Content.Shared.Corvax.CCCVars;
using Content.Shared._WL.Barks; // WL-Changes
using Content.Shared.Corvax.TTS;
using Content.Client.UserInterface.Systems.Chat; // WL-Changes
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface; // WL-Changes
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Client.Corvax.TTS;

/// <summary>
/// Plays TTS audio in world
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IResourceManager _res = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private IUserInterfaceManager _ui = default!; // WL-Changes

    private ISawmill _sawmill = default!;
    private static MemoryContentRoot _contentRoot = new();
    private static readonly ResPath Prefix = ResPath.Root / "TTS";

    private static bool _contentRootAdded;

    // WL-Changes-Start: Correct whisper attenuation units
    /// <summary>
    /// Reducing the volume of the TTS when whispering, in decibels.
    /// </summary>
    private const float WhisperFade = 4f;
    // WL-Changes-End

    /// <summary>
    /// The volume at which the TTS sound will not be heard.
    /// </summary>
    private const float MinimalVolume = -10f;

    private float _volume = 0.0f;
    private int _fileIdx = 0;

    public override void Initialize()
    {
        if (!_contentRootAdded)
        {
            _contentRootAdded = true;
            _res.AddRoot(Prefix, _contentRoot);
        }

        _sawmill = Logger.GetSawmill("tts");
        _cfg.OnValueChanged(CCCVars.TTSVolume, OnTtsVolumeChanged, true);
        SubscribeNetworkEvent<PlayTTSEvent>(OnPlayTTS);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(CCCVars.TTSVolume, OnTtsVolumeChanged);
    }

    public void RequestPreviewTTS(string voiceId, string previewText)
    {
        RaiseNetworkEvent(new RequestPreviewTTSEvent(voiceId, previewText)); //WL-PreviewTTSEdit
    }

    private void OnTtsVolumeChanged(float volume)
    {
        _volume = volume;
    }

    private void OnPlayTTS(PlayTTSEvent ev)
    {
        // WL-Changes-Start: Speech mode
        if (!ev.IsPreview && _cfg.GetCVar(WLCVars.SpeechMode) != SpeechMode.Tts)
            return;
        // WL-Changes-End

        _sawmill.Verbose($"Play TTS audio {ev.Data.Length} bytes from {ev.SourceUid} entity");

        var filePath = new ResPath($"{_fileIdx++}.ogg");
        _contentRoot.AddOrUpdateFile(filePath, ev.Data);

        var audioResource = new AudioResource();
        audioResource.Load(IoCManager.Instance!, Prefix / filePath);

        var audioParams = AudioParams.Default
            .WithVolume(AdjustVolume(ev.IsWhisper))
            .WithMaxDistance(AdjustDistance(ev.IsWhisper));

        var soundSpecifier = new ResolvedPathSpecifier(Prefix / filePath);

        if (ev.SourceUid != null)
        {
            var sourceUid = GetEntity(ev.SourceUid.Value);

            if (!Exists(sourceUid) || Deleted(sourceUid))
            {
                _contentRoot.RemoveFile(filePath);
                return;
            }

            var stream = _audio.PlayEntity(audioResource.AudioStream, sourceUid, soundSpecifier, audioParams);

            // WL-Changes-Start: Keep the overhead message visible for the full TTS playback
            if (stream != null)
            {
                _ui.GetUIController<ChatUIController>()
                    .KeepSpeechBubbleVisible(sourceUid, audioResource.AudioStream.Length);
            }
            // WL-Changes-End
        }
        else
        {
            _audio.PlayGlobal(audioResource.AudioStream, soundSpecifier, audioParams);
        }

        _contentRoot.RemoveFile(filePath);
    }

    private float AdjustVolume(bool isWhisper)
    {
        var volume = MinimalVolume + SharedAudioSystem.GainToVolume(_volume);

        if (isWhisper)
        {
            volume -= WhisperFade; // WL-Changes
        }

        return volume;
    }

    private float AdjustDistance(bool isWhisper)
    {
        return isWhisper ? SharedChatSystem.WhisperMuffledRange : SharedChatSystem.VoiceRange;
    }
}
