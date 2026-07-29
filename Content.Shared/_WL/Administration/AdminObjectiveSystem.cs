using Content.Shared.Objectives.Components;

namespace Content.Shared._WL.Administration;

public sealed class AdminObjectiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AdminObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, AdminObjectiveComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = comp.Progress;
    }
}
