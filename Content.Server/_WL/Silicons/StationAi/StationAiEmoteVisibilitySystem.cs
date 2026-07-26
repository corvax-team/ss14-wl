using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Examine;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Player;

namespace Content.Server.Silicons.StationAi;

public sealed partial class StationAiEmoteVisibilitySystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private SharedStationAiSystem _stationAiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandRecipients);
    }

    /// <summary>
    /// Ai gets the player's emotion if it can examine him and the player is within 10 tiles
    /// </summary>
    private void OnExpandRecipients(ExpandICChatRecipientsEvent ev)
    {

        if (ev.ChatType != InGameICChatType.Emote)
        {
            return;
        }

        var coreQuery = EntityQueryEnumerator<StationAiCoreComponent>();
        var range = 10;

        while (coreQuery.MoveNext(out var coreUid, out var core))
        {

            if (!_stationAiSystem.TryGetHeld((coreUid, core), out var heldAi))
                continue;

            if (!TryComp<ActorComponent>(heldAi.Value, out var actor))
                continue;

            if (actor.PlayerSession == null)
                continue;

            if (!core.CanSeeEmote)
                continue;

            if (core.RemoteEntity is not { } remoteEntity)
                continue;

            if (!_examine.InRangeUnOccluded(remoteEntity, ev.Source, range))
                continue;

            if (!_examine.CanExamine(heldAi.Value, ev.Source))
                continue;

            ev.Recipients[actor.PlayerSession] = new ChatSystem.ICChatRecipientData(
                Range: 10f,
                Observer: false
            );

        }
    }
}
