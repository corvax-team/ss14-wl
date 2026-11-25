using Content.Shared.Interaction.Events;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._WL.Parry;

public abstract partial class SharedParrySystem
{
    public void InitializeParryStatusEffects()
    {
        base.Initialize();

        SubscribeLocalEvent<AttackProhibitionStatusEffectComponent, StatusEffectRelayedEvent<AttackAttemptEvent>>(OnAttackProhibitionStatusEffectAttackAttempt);
    }

    private void OnAttackProhibitionStatusEffectAttackAttempt(Entity<AttackProhibitionStatusEffectComponent> ent,
        ref StatusEffectRelayedEvent<AttackAttemptEvent> args)
    {
        args.Args.Cancel();
    }
}
