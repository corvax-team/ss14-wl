using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted; // WL-Offmed

namespace Content.Shared._Offbrand.StatusEffects;

public sealed class PopupOnAppliedStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PopupOnAppliedStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
    }

    private void OnStatusEffectApplied(Entity<PopupOnAppliedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        // WL-Offmed: add PainNumbness for popup about pain
        var message = (string)ent.Comp.Message;
        if (!EntityManager.HasComponent<PainNumbnessComponent>(ent.Owner)
            || !message.EndsWith("-pain-applied")
            || ent.Comp.Message == "blackout-pain-applied")
            _popup.PopupClient(Loc.GetString(ent.Comp.Message), args.Target, args.Target, ent.Comp.VisualType);
    }
}
