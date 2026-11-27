using Content.Server.Shuttles.Systems;
using Content.Shared._Ship14.CorticalBorer.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Robust.Server.Player;

namespace Content.Server._Ship14.CorticalBorer;

public sealed partial class CorticalBorerObjectiveSystem : EntitySystem
{
    [Dependency] private EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CorticalBorerObjectiveComponent, ObjectiveAfterAssignEvent>(OnAssigned);
        SubscribeLocalEvent<CorticalBorerObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAssigned(Entity<CorticalBorerObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        var required = Math.Max(CorticalBorerObjectiveComponent.MinimumRequiredInfected, _playerManager.PlayerCount / 7);
        ent.Comp.RequiredInfected = required;
        _metaData.SetEntityName(ent, Loc.GetString("objective-cortical-borer-name", ("count", required)), args.Meta);
        _metaData.SetEntityDescription(ent, Loc.GetString("objective-cortical-borer-description", ("count", required)), args.Meta);
    }

    private void OnGetProgress(Entity<CorticalBorerObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var infected = 0;
        var query = EntityQueryEnumerator<CorticalBorerHostComponent>();
        while (query.MoveNext(out var host, out var component))
        {
            if (component.Borer == null || !_mobState.IsAlive(host) || !_emergencyShuttle.IsTargetEscaping(host))
                continue;

            infected++;
        }

        args.Progress = Math.Clamp((float) infected / ent.Comp.RequiredInfected, 0f, 1f);
    }
}
