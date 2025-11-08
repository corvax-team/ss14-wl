using System.Threading.Tasks;
using Content.Shared.Chat;
using Content.Server.Chat.Systems;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.GameTicking;
using Content.Shared.Players.RateLimiting;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._WL.Languages;

/// <summary>
/// Play languages sounds
/// </summary>
public sealed class LanguagesSoundsSystem : EntitySystem
{
    [Dependency] private readonly LanguagesSystem _languages = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    public override void Initialize()
    {
        //base.Initialize();

        SubscribeLocalEvent<LanguagesComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(EntityUid uid, LanguagesComponent component, EntitySpokeEvent args)
    {
        var protoId = component.CurrentLanguage;
        var proto = _languages.GetLanguagePrototype(protoId);

        if (proto is null)
            return;

        if (args.LangMessage != null)
        {
            if (args.LangObfusMessage != null && args.ObfuscatedMessage != null)
                HandleSay(uid, protoId, proto, true);
            else
                HandleSay(uid, protoId, proto);
        }
    }

    private void HandleSay(EntityUid uid, ProtoId<LanguagePrototype> protoId, LanguagePrototype proto, bool isWhisper = false)
    {
        var soundEvent = new LanguageSoundEvent(protoId, GetNetEntity(uid));
        var whispSoundEvent = new LanguageSoundEvent(protoId, GetNetEntity(uid), true);

        if (!proto.NeedSound)
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

            var check = _languages.CanUnderstand(uid, listener);

            if (check) continue;
            RaiseNetworkEvent(!isWhisper ? soundEvent: whispSoundEvent, session);
        }
    }
}
