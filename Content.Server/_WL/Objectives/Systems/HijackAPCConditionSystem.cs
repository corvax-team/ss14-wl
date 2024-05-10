using Content.Server.Power.Components;
using Content.Server._WL.PulseDemon.Components;
using Content.Shared.Mind;
using Content.Server._WL.Objectives.Components;
using Content.Shared.Objectives.Components;
using System.Linq;

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

        var apcs = EntityQuery<ApcComponent, TransformComponent>()
            .Where(apc => apc.Item2.GridUid == gridUid);

        var hijackedApcs = apcs.Where(hijacked => HasComp<HijackedByPulseDemonComponent>(hijacked.Item2.Owner));

        var apcsCount = apcs.Count();
        var hijackedApcsCount = (float)hijackedApcs.Count();

        return apcsCount == 0
            ? 1f
            : hijackedApcsCount / apcsCount;
    }
}
