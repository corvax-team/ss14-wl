using System.Linq;
using Content.Shared._WL.Body;
using Content.Shared.Body;

namespace Content.Server._WL.Body;

public sealed partial class HideUnderClothingSystem : SharedHideUnderClothingSystem
{
    private const string HideMarkingsAction = "ActionHideMarkingsUnderClothing";

    private readonly HashSet<EntityUid> _pending = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VisualBodyComponent, OrganInsertedIntoEvent>(OnInserted);
        SubscribeLocalEvent<VisualBodyComponent, OrganRemovedFromEvent>(OnRemoved);
        SubscribeLocalEvent<VisualOrganMarkingsComponent, OrganMarkingsChangedEvent>(OnMarkingsChanged);
        SubscribeLocalEvent<HideUnderClothingComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(Entity<BodyComponent> ent, ref MapInitEvent args) => _pending.Add(ent);
    private void OnInserted(Entity<VisualBodyComponent> ent, ref OrganInsertedIntoEvent args) => _pending.Add(ent);
    private void OnRemoved(Entity<VisualBodyComponent> ent, ref OrganRemovedFromEvent args) => _pending.Add(ent);

    private void OnMarkingsChanged(Entity<VisualOrganMarkingsComponent> ent, ref OrganMarkingsChangedEvent args)
    {
        if (TryComp<OrganComponent>(ent, out var organ) && organ.Body is { } body)
            _pending.Add(body);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var body in _pending.ToArray())
        {
            _pending.Remove(body);
            if (!TerminatingOrDeleted(body) && HasComp<VisualBodyComponent>(body))
                Refresh(body);
        }
    }

    private void Refresh(EntityUid body)
    {
        if (!HasEligibleMarkings(body))
        {
            if (TryComp<HideUnderClothingComponent>(body, out var old))
            {
                SetHidden((body, old), false);
                Actions.RemoveAction(body, old.Action);
                old.Action = null;
                Dirty(body, old);
            }
            return;
        }

        var comp = EnsureComp<HideUnderClothingComponent>(body);
        if (comp.Action == null || Deleted(comp.Action))
        {
            Actions.AddAction(body, ref comp.Action, HideMarkingsAction);
            Dirty(body, comp);
        }
        UpdateClothing((body, comp));
    }

    private void OnShutdown(Entity<HideUnderClothingComponent> ent, ref ComponentShutdown args) =>
        Actions.RemoveAction(ent.Owner, ent.Comp.Action);
}
