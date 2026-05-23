using Content.Shared.Chat;
using Content.Server.Chat.Systems;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;

namespace Content.Server._WL.Languages;

/// <summary>
/// Play languages sounds
/// </summary>
public sealed partial class LanguagesSoundsSystem : EntitySystem
{
    [Dependency] private LanguagesSystem _languages = default!;
    [Dependency] private SharedTransformSystem _xforms = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        //base.Initialize();

        SubscribeLocalEvent<LanguagesComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(EntityUid uid, LanguagesComponent component, EntitySpokeEvent args)
    {
        var msg = args.LangMessage;
        if (msg == null)
            return;

        var protoId = _languages.GetLanguagePrototype(uid, msg)?.ID ?? component.CurrentLanguage;
        if (protoId == null)
            return;

        var proto = _protoMan.Index<LanguagePrototype>(protoId);

        var isWhisper = args.LangObfusMessage != null && args.ObfuscatedMessage != null;
        HandleSay(uid, proto, isWhisper);
    }

    private void HandleSay(EntityUid uid, LanguagePrototype proto, bool isWhisper = false)
    {
        var soundEvent = new LanguageSoundEvent(proto.ID, GetNetEntity(uid));
        var whispSoundEvent = new LanguageSoundEvent(proto.ID, GetNetEntity(uid), true);

        if (!proto.CustomSound)
            return;

        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue) continue;
            var listener = session.AttachedEntity.Value;
            var xform = xformQuery.GetComponent(listener);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();
            if (distance > ChatSystem.VoiceRange)
                continue;

            if (distance > ChatSystem.VoiceRange * ChatSystem.VoiceRange && isWhisper)
                continue;

            var check = _languages.CanUnderstand(uid, listener, overrideLang: proto.ID);

            if (check) continue;
            RaiseNetworkEvent(!isWhisper ? soundEvent : whispSoundEvent, session);
        }
    }
}
