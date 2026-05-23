using Content.Server.Power.Components;
using Content.Server._WL.PulseDemon.Components;
using Content.Shared.Mind;
using Content.Server._WL.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._WL.Objectives.Systems;

public sealed class HijackAPCConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HijackAPCConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, HijackAPCConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind);
    }

    private float GetProgress(MindComponent mind)
    {
        if (mind.OwnedEntity == null)
            return 0f;

        var gridUid = Transform(mind.OwnedEntity.Value).GridUid;

        var apcsCount = 0f;
        var hijackedApcsCount = 0f;

        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var transform))
        {
            if (transform.GridUid != gridUid) continue;

            apcsCount += 1;

            if (HasComp<HijackedByPulseDemonComponent>(uid)) hijackedApcsCount += 1;
        }

        return apcsCount == 0 ? 1f : hijackedApcsCount / apcsCount;
    }
}
