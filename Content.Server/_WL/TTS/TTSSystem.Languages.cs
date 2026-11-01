using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server._WL.Languages;
using Content.Shared.Chat;
using Content.Shared.Corvax.TTS;
using Robust.Shared.Player;

namespace Content.Server.Corvax.TTS;

public sealed partial class TTSSystem
{
    private readonly SemaphoreSlim _languageTtsRequests = new(4);
    private const int MaxPendingLanguageSpeech = 64;
    private int _pendingLanguageSpeech;

    [SubscribeLocalEvent]
    private void OnLanguageSpeech(Entity<TTSComponent> ent, ref LanguageSpeechEvent args)
    {
        if (!_isEnabled || string.IsNullOrEmpty(ent.Comp.VoicePrototypeId) ||
            !ProtoMan.TryIndex(args.Language, out var language))
            return;

        var voice = new TransformSpeakerVoiceEvent(ent, ent.Comp.VoicePrototypeId);
        RaiseLocalEvent(ent, voice);
        if (!ProtoMan.TryIndex<TTSVoicePrototype>(voice.VoiceId, out var prototype))
            return;

        var range = args.Whisper ? SharedChatSystem.WhisperMuffledRange : SharedChatSystem.VoiceRange;
        var nearby = GetReceiversFilter(ent.Owner, range);
        var groups = new Dictionary<string, List<(EntityUid Entity, ICommonSession Session)>>();
        foreach (var (listener, message) in args.Messages)
        {
            if (message.Length > MaxMessageChars || !TryComp<ActorComponent>(listener, out var actor))
                continue;

            var session = actor.PlayerSession;
            if (!nearby.Recipients.Contains(session) || !language.NeedTTS && !_languages.CanUnderstand(args.Language, listener))
                continue;

            if (!groups.TryGetValue(message, out var recipients))
            {
                recipients = new();
                groups.Add(message, recipients);
            }

            recipients.Add((listener, session));
        }

        foreach (var (message, recipients) in groups)
        {
            if (_pendingLanguageSpeech >= MaxPendingLanguageSpeech)
                break;

            _pendingLanguageSpeech++;
            _ = PlayLanguageSpeech(ent.Owner, message, prototype.Speaker, args.Whisper, range, recipients);
        }
    }

    private async Task PlayLanguageSpeech(EntityUid source, string message, string speaker, bool whisper,
        float range, List<(EntityUid Entity, ICommonSession Session)> recipients)
    {
        var acquired = false;
        try
        {
            acquired = await _languageTtsRequests.WaitAsync(TimeSpan.FromSeconds(5));
            if (!acquired)
                return;

            if (!_isEnabled || TerminatingOrDeleted(source))
                return;

            var sound = await GenerateTTS(message, speaker, whisper);
            if (sound == null || !_isEnabled || TerminatingOrDeleted(source))
                return;

            var nearby = GetReceiversFilter(source, range);
            var filter = Filter.Empty();
            foreach (var (entity, session) in recipients)
            {
                if (session.AttachedEntity == entity && nearby.Recipients.Contains(session))
                    filter.AddPlayer(session);
            }

            if (filter.Count > 0)
                RaiseNetworkEvent(new PlayTTSEvent(sound, GetNetEntity(source), whisper), filter, recordReplay: false);
        }
        catch (Exception exception)
        {
            Log.Error($"Language TTS playback failed: {exception}");
        }
        finally
        {
            if (acquired)
                _languageTtsRequests.Release();

            _pendingLanguageSpeech--;
        }
    }
}
