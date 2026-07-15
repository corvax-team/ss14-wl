using Content.Server.Station.Systems;
using Content.Shared._WL.SafeCode;
using Content.Shared._WL.SafeCode.CapsSpareSafe;
using Content.Shared.Paper;
using Content.Shared.Station.Components;
using Robust.Shared.Random;

namespace Content.Server._WL.SafeCode.SafeCodeNote;

public sealed partial class SafeCodeNoteSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PaperSystem _paper = default!;
    [Dependency] private StationSystem _station = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SafeCodeNoteComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(Entity<SafeCodeNoteComponent> ent, ref MapInitEvent args)
    {
        var candidates = new List<Entity<SafeCodeComponent>>();

        var query = EntityQueryEnumerator<CapsSpareSafeComponent, SafeCodeComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var safeCode, out var xform))
        {
            if (xform.GridUid == null)
                continue;

            if (!HasComp<StationMemberComponent>(xform.GridUid.Value))
                continue;

            candidates.Add((uid, safeCode));
        }

        if (candidates.Count == 0)
            return;

        var chosen = _random.Pick(candidates);
        var text = Loc.GetString(ent.Comp.NoteText, ("code", chosen.Comp.Code));

        _paper.SetContent(ent.Owner, text);
    }
}
