using Content.Shared._WL.Barks;
using Content.Shared._WL.CCVars;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;

namespace Content.Client._WL.Barks;

public sealed partial class SpeechSoundSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PlaySpeechSoundEvent>(OnPlaySpeechSound);
    }

    private void OnPlaySpeechSound(PlaySpeechSoundEvent ev)
    {
        if (_player.LocalSession == null ||
            !TryGetEntity(ev.Source, out var source) ||
            Transform(source.Value).MapID == MapId.Nullspace)
            return;

        // Bark voices already provide the complete speech sound. Mixing the
        // species speech noise on top makes in-round speech differ from the
        // lobby preview and is especially noticeable for vox characters.
        if (_cfg.GetCVar(WLCVars.SpeechMode) == SpeechMode.Barks)
            return;

        _audio.PlayEntity(ev.Sound, _player.LocalSession, source.Value, ev.Sound.Params);
    }
}
