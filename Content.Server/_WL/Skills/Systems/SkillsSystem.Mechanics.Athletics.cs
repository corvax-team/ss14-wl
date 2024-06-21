using Content.Server.Hands.Systems;
using Content.Shared._WL.Skills.Components.Mechanics;
using Content.Shared._WL.Skills.Systems;
using Content.Shared._WL.Stamina;
using Content.Shared.Climbing.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Prying.Components;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;

namespace Content.Server._WL.Skills.Systems
{
    public sealed partial class SkillsSystem : SharedSkillsSystem
    {
        [Dependency] private readonly HandsSystem _hands = default!;

        public void InitializeAthleticsMechanics()
        {
            SubscribeLocalEvent<AthleticSkillComponent, MapInitEvent>(OnAthleticMapInit);
            SubscribeLocalEvent<AthleticSkillComponent, BeforeThrowEvent>(OnAthleticThrow);

            SubscribeLocalEvent<PhysicsComponent, GetPryTimeModifierEvent>(OnPry);
            SubscribeLocalEvent<PhysicsComponent, AttemptClimbEvent>(OnAthleticClimb);
        }

        private void OnAthleticMapInit(EntityUid holder, AthleticSkillComponent comp, MapInitEvent args)
        {
            if (TryComp<StaminaWasterComponent>(holder, out var staminaWasterComp))
            {
                staminaWasterComp.StaminaPerSecond = comp.StaminaWasteValue;
                staminaWasterComp.ThrowPenaltyForOneMassUnit *= comp.ThrowStaminaWasteModifier;
            }

            if (TryComp<StaminaComponent>(holder, out var staminaComp))
            {
                staminaComp.CritThreshold *= comp.StaminaModifier;
                staminaComp.Decay = comp.StaminaRegenerateValue;
                Dirty(holder, staminaComp);
            }

            if (TryComp<HandsComponent>(holder, out var handsComp))
                _hands.ChangeThrowRange((holder, handsComp), handsComp.ThrowRange * comp.ThrowRangeModifier);

            if (TryComp<PullerComponent>(holder, out var pullerComp))
                PullingSystem.ChangeBaseWalkingSpeeds(
                    (holder, pullerComp),
                    pullerComp.BaseWalkSpeedModifier * comp.PullWalkSpeedModifier,
                    pullerComp.BaseSprintSpeedModifier * comp.PullSprintSpeedModifier);

        }

        private void OnAthleticThrow(EntityUid holder, AthleticSkillComponent comp, ref BeforeThrowEvent args)
        {
            var thrown = args.ItemUid;
            if (TryComp<ItemComponent>(thrown, out var itemComp))
                if (itemComp.Size == comp.BlockedItemSize)
                    args.Cancelled = true;

            args.ThrowStrength *= comp.ThrowForceModifier;
        }

        private void OnPry(EntityUid airlock, PhysicsComponent comp, ref GetPryTimeModifierEvent args)
        {
            if (!TryComp<AthleticSkillComponent>(args.User, out var athlComp))
                return;

            args.BaseTime *= athlComp.PryTimeModifier;
        }

        private void OnAthleticClimb(EntityUid climbable, PhysicsComponent physicComp, ref AttemptClimbEvent args)
        {
            if (!TryComp<AthleticSkillComponent>(args.User, out var comp))
                return;

            args.DoAfterTime *= comp.ClimbTimeModifier;
        }
    }
}
