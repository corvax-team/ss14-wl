using Content.Server.Mind;
using Content.Server.StationEvents.Components;
using Content.Shared.Antag;
using Content.Shared.GameTicking.Rules;
using Content.Shared.Roles;
using Content.Shared.Station.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Ship14.CorticalBorer;

public sealed partial class CorticalBorerRuleSystem : GameRuleSystem<CorticalBorerRuleComponent>
{
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private SharedRoleSystem _roles = default!;
    [Dependency] private TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CorticalBorerRuleComponent, AntagSelectLocationEvent>(OnSelectLocation);
        SubscribeLocalEvent<RoleAddedEvent>(OnRoleAdded);
    }

    private void OnRoleAdded(RoleAddedEvent args)
    {
        if (!_roles.MindHasRole<CorticalBorerRoleComponent>(args.MindId) ||
            _mind.TryGetObjectiveComp<CorticalBorerObjectiveComponent>(args.MindId, out _, args.Mind))
            return;

        _mind.TryAddObjective(args.MindId, args.Mind, "CorticalBorerObjective");
    }

    private void OnSelectLocation(Entity<CorticalBorerRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        var vents = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        while (vents.MoveNext(out _, out _, out var transform))
        {
            if (!transform.Anchored || transform.GridUid is not { } grid ||
                !HasComp<StationMemberComponent>(grid))
                continue;

            args.Coordinates.Add(_transform.ToMapCoordinates(transform.Coordinates));
        }
    }
}
