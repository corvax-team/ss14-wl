using System.Text;
using Content.Server._WL.Languages;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Speech.Components;
using Content.Shared._Goobstation.TapeRecorder;
using Content.Shared._Goobstation.TapeRecorder.Components;
using Content.Shared._Goobstation.TapeRecorder.Systems;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Chat;
using Content.Shared._WL.Barks; // WL-Changes
using Content.Shared.Corvax.TTS;
using Content.Shared.Paper;
using Content.Shared.Speech;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.TapeRecorder;

public sealed partial class TapeRecorderSystem : SharedTapeRecorderSystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private PaperSystem _paper = default!;
    [Dependency] private LanguagesSystem _language = default!; // WL-Languages

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TapeRecorderComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<TapeRecorderComponent, PrintTapeRecorderMessage>(OnPrintMessage);
    }

    /// <summary>
    /// Given a time range, play all messages on a tape within said range, [start, end).
    /// Split into this system as shared does not have ChatSystem access
    /// </summary>
    protected override void ReplayMessagesInSegment(Entity<TapeRecorderComponent> ent, TapeCassetteComponent tape, float segmentStart, float segmentEnd)
    {
        var voice = EnsureComp<VoiceOverrideComponent>(ent);
        var speech = EnsureComp<SpeechComponent>(ent);

        foreach (var message in tape.RecordedData)
        {
            if (message.Timestamp < tape.CurrentPosition || message.Timestamp >= segmentEnd)
                continue;

            //Change the voice to match the speaker
            voice.NameOverride = message.Name ?? ent.Comp.DefaultName;
            // TODO: mimic the exact string chosen when the message was recorded
            var verb = message.Verb ?? SharedChatSystem.DefaultSpeechVerb;
            speech.SpeechVerb = _proto.Index<SpeechVerbPrototype>(verb);

            // WL-Changes-Start
            if (TryComp<TTSComponent>(ent, out var tts))
                tts.VoicePrototypeId = message.TTS;

            // WL-Changes-Start: Speech barks
            if (!string.IsNullOrEmpty(message.BarkVoice) &&
                _proto.HasIndex<BarkPrototype>(message.BarkVoice))
            {
                EnsureComp<SpeechBarksComponent>(ent);
                voice.BarkVoiceOverride = message.BarkVoice;
                voice.BarkPitchOverride = message.BarkPitch;
                voice.BarkMinDelayOverride = message.BarkMinDelay;
                voice.BarkMaxDelayOverride = message.BarkMaxDelay;
            }
            else
            {
                RemComp<SpeechBarksComponent>(ent);
                voice.BarkVoiceOverride = null;
                voice.BarkPitchOverride = null;
                voice.BarkMinDelayOverride = null;
                voice.BarkMaxDelayOverride = null;
            }
            // WL-Changes-End

            if (TryComp<LanguagesComponent>(ent, out var languageComp))
            {
                // I already know that's a bad way to do it
                languageComp.CurrentLanguage = message.Language != "Translate"
                    ? message.Language
                    : "Translate";
            }
            // WL-Changes-end

            //Play the message
            _chat.TrySendInGameICMessage(ent, message.Message, InGameICChatType.Speak, false);
        }
    }

    /// <summary>
    /// Whenever someone speaks within listening range, record it to tape
    /// </summary>
    private void OnListen(Entity<TapeRecorderComponent> ent, ref ListenEvent args)
    {
        // mode should never be set when it isn't active but whatever
        if (ent.Comp.Mode != TapeRecorderMode.Recording || !HasComp<ActiveTapeRecorderComponent>(ent))
            return;

        // No feedback loops
        if (args.Source == ent.Owner)
            return;

        if (!TryGetTapeCassette(ent, out var cassette))
            return;

        // TODO: Handle "Someone" when whispering from far away, needs chat refactor

        //Handle someone using a voice changer
        var nameEv = new TransformSpeakerNameEvent(args.Source, Name(args.Source));
        RaiseLocalEvent(args.Source, nameEv);

        //Add a new entry to the tape
        var verb = _chat.GetSpeechVerb(args.Source, args.Message);
        var name = nameEv.VoiceName;

        // WL-Changes-Start
        var language = "Translate";
        var tts = string.Empty;
        var barkVoice = string.Empty;
        var barkPitch = SpeechBarksComponent.DefaultPitch;
        var barkMinDelay = SpeechBarksComponent.DefaultMinDelay;
        var barkMaxDelay = SpeechBarksComponent.DefaultMaxDelay;

        if (TryComp<LanguagesComponent>(args.Source, out var languagesSpeaker) && languagesSpeaker.CurrentLanguage.HasValue)
            language = languagesSpeaker.CurrentLanguage;

        if (TryComp<TTSComponent>(args.Source, out var ttsComp))
            tts = ttsComp.VoicePrototypeId ?? "";

        if (TryComp<SpeechBarksComponent>(args.Source, out var barkComp))
        {
            var barkTransform = new TransformSpeakerBarkEvent(
                args.Source,
                barkComp.Voice,
                barkComp.Pitch,
                barkComp.MinDelay,
                barkComp.MaxDelay);
            RaiseLocalEvent(args.Source, barkTransform);
            barkVoice = barkTransform.Voice;
            barkPitch = barkTransform.Pitch;
            barkMinDelay = barkTransform.MinDelay;
            barkMaxDelay = barkTransform.MaxDelay;
        }
        // WL-Changes: added language, TTS and speech bark support
        cassette.Comp.Buffer.Add(new TapeCassetteRecordedMessage(
            cassette.Comp.CurrentPosition,
            name,
            verb,
            args.Message,
            language,
            tts,
            barkVoice,
            barkPitch,
            barkMinDelay,
            barkMaxDelay));
        // WL-Changes-end
    }

    private void OnPrintMessage(Entity<TapeRecorderComponent> ent, ref PrintTapeRecorderMessage args)
    {
        var (uid, comp) = ent;

        if (comp.CooldownEndTime > Timing.CurTime)
            return;

        if (!TryGetTapeCassette(ent, out var cassette))
            return;

        var text = new StringBuilder();
        var paper = Spawn(comp.PaperPrototype, Transform(ent).Coordinates);

        // Sorting list by time for overwrite order
        // TODO: why is this needed? why wouldn't it be stored in order
        var data = cassette.Comp.RecordedData;
        data.Sort((x,y) => x.Timestamp.CompareTo(y.Timestamp));

        // Looking if player's entity exists to give paper in its hand
        var player = args.Actor;
        if (Exists(player))
            _hands.PickupOrDrop(player, paper, checkActionBlocker: false);

        if (!TryComp<PaperComponent>(paper, out var paperComp))
            return;

        Audio.PlayPvs(comp.PrintSound, ent);

        text.AppendLine(Loc.GetString("tape-recorder-print-start-text"));
        text.AppendLine();
        foreach (var message in cassette.Comp.RecordedData)
        {
            var name = message.Name ?? ent.Comp.DefaultName;
            var time = TimeSpan.FromSeconds((double) message.Timestamp);

            // WL-Languages-Start
            if (message.Language != "Translate")
            {
                var language = _language.GetLanguagePrototype(message.Language);
                message.Message = language.Obfuscation.Obfuscate(message.Message, 634);
            }
            // WL-Languages-End

            text.AppendLine(Loc.GetString("tape-recorder-print-message-text",
                ("time", time.ToString(@"hh\:mm\:ss")),
                ("source", name),
                ("message", message.Message)));
        }
        text.AppendLine();
        text.Append(Loc.GetString("tape-recorder-print-end-text"));

        _paper.SetContent((paper, paperComp), text.ToString());

        comp.CooldownEndTime = Timing.CurTime + comp.PrintCooldown;
    }
}
