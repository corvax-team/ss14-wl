using Content.Shared.EntityEffects;

namespace Content.Shared._WL.EntityEffects.Effects;

/// <summary>
/// Effect that prepares an item for flammable
/// </summary>
public sealed partial class LavaPrepareFlammable : EntityEffectBase<LavaPrepareFlammable>
{
    public override void RaiseEvent(EntityUid uid, IEntityEffectRaiser? raiser, float scale, EntityUid? target)
    {
        var ev = new EntityEffectEvent<LavaPrepareFlammable>(this, scale, target);
        IoCManager.Resolve<IEntityManager>().EventBus.RaiseLocalEvent(uid, ref ev);
    }
}
