using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Popups;
using Content.Shared.Gibbing;
using Content.Shared.StatusEffectNew;
using Content.Shared.FixedPoint;

namespace Content.Shared._WL._Offbrand.StatusEffects;

public sealed partial class GibOnDamageStatusEffectSystem : EntitySystem
{
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GibOnDamageStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
    }

    private void OnStatusEffectApplied(Entity<GibOnDamageStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<DamageableComponent>(args.Target, out var damageable))
            return;

        var dict = _damageable.GetAllDamage((args.Target, damageable)).DamageDict;
;
        if (ent.Comp.MinimumDamage > dict.GetValueOrDefault(ent.Comp.DamageType, FixedPoint2.Zero))
            return;

        if (ent.Comp.ShowPopup)
            _popupSystem.PopupCoordinates(Loc.GetString(ent.Comp.PopupText, ("target", args.Target)),
                Transform(args.Target).Coordinates,
                PopupType.LargeCaution);

        _gibbing.Gib(args.Target);
    }
}
