using Content.Shared.Chat;
using Content.Shared._WL.Languages;
using Content.Client._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Corvax.CCCVars;
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Client._WL.Languages;

/// <summary>
/// Play languages sounds
/// </summary>
public sealed class LanguagesSoundsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ClientLanguagesSystem _languages = default!;

    private ISawmill _sawmill = default!;

    /// <summary>
    /// Reducing the volume of the sound when whispering. Will be converted to logarithm.
    /// </summary>
    private const float WhisperFade = 4f;

    /// <summary>
    /// The volume at which the sound sound will not be heard.
    /// </summary>
    private const float MinimalVolume = -10f;

    private float _volume = 0.0f;
    private int _fileIdx = 0;

    public override void Initialize()
    {
        //base.Initialize();

        _sawmill = Logger.GetSawmill("lang.sound");
        _cfg.OnValueChanged(CCCVars.TTSVolume, OnTtsVolumeChanged, true);
        SubscribeNetworkEvent<LanguageSoundEvent>(OnLanguageSound);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(CCCVars.TTSVolume, OnTtsVolumeChanged);
    }

    private void OnTtsVolumeChanged(float volume)
    {
        _volume = volume;
    }

    private void OnLanguageSound(LanguageSoundEvent ev)
    {
        _sawmill.Verbose($"Play language sound audio {ev.Language} bytes from {ev.SourceUid} entity");
        var proto = _languages.GetLanguagePrototype(ev.Language);

        if (proto is null)
            return;

        //var filePath = ResPath.Root / proto.Sound.Path;

        //var audioResource = new AudioResource();
        //audioResource.Load(IoCManager.Instance!, filePath);

        var audioParams = AudioParams.Default
            .WithVolume(AdjustVolume(ev.IsWhisper))
            .WithMaxDistance(AdjustDistance(ev.IsWhisper));

        //var soundSpecifier = new ResolvedPathSpecifier(filePath);
        var soundSpecifier = proto.Sound;

        if (ev.SourceUid != null)
        {
            if (!TryGetEntity(ev.SourceUid.Value, out _))
                return;
            var sourceUid = GetEntity(ev.SourceUid.Value);
            //Logger.Debug(soundSpecifier.Path.ToString());
            _audio.PlayPredicted(soundSpecifier, sourceUid, sourceUid, audioParams); //, soundSpecifier, audioParams);
        }
        else
        {
            // _audio.PlayGlobal(soundSpecifier, audioParams);
        }
    }

    private float AdjustVolume(bool isWhisper)
    {
        var volume = MinimalVolume + SharedAudioSystem.GainToVolume(_volume);

        if (isWhisper)
        {
            volume -= SharedAudioSystem.GainToVolume(WhisperFade);
        }

        return volume;
    }

    private float AdjustDistance(bool isWhisper)
    {
        return isWhisper ? SharedChatSystem.WhisperMuffledRange : SharedChatSystem.VoiceRange;
    }
}
