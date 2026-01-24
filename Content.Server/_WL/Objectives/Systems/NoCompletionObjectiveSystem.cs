using Content.Server._WL.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._WL.Objectives.Systems;

public sealed class NoCompletionObjectiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoCompletionObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<NoCompletionObjectiveComponent> entity, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f;
    }
}
