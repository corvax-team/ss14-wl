using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Damage;

public sealed partial class DamageStatisticsSystemSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent]
    private void OnDamageDealt(Entity<DamageStatisticsComponent> ent, ref DamageDealtEvent args)
    {
        ent.Comp.DamagePerTime += args.Damage;
        ent.Comp.TotalDamage += args.Damage;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DamageStatisticsComponent>();

        while (query.MoveNext(out var uid, out var stats))
        {
            if (stats.NextUpdateTime > _timing.CurTime)
                continue;

            stats.NextUpdateTime = _timing.CurTime + stats.UpdateTime;

            stats.DamagePerTime = new DamageSpecifier();
        }
    }
}
