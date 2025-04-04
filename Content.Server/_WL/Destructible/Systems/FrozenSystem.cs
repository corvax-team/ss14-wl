using Content.Server._WL.Destructible.Components;
using Content.Server.Humanoid;
using Content.Shared.Cloning;
using Content.Shared.Cloning.Events;
using Content.Shared.Damage;
using Content.Shared.HealthExaminable;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Rejuvenate;

namespace Content.Server._WL.Destructible.Systems
{
    public sealed partial class FrozenSystem : EntitySystem
    {
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly HumanoidAppearanceSystem _appearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FrozenComponent, RefreshNameModifiersEvent>(OnRefreshName);
            SubscribeLocalEvent<FrozenComponent, BeforeDamageChangedEvent>(BeforeDamageChanged);
            SubscribeLocalEvent<FrozenComponent, CloningEvent>(OnClone);
            SubscribeLocalEvent<FrozenComponent, HealthBeingExaminedEvent>(OnHealthExamine);
            SubscribeLocalEvent<FrozenComponent, RejuvenateEvent>(OnRejuvenate);
        }

        private void OnRefreshName(EntityUid ent, FrozenComponent comp, RefreshNameModifiersEvent args)
        {
            args.AddModifier(comp.FrozenPrefix);
            args.AddModifier(comp.BaseName, int.MinValue);
        }

        private void BeforeDamageChanged(EntityUid _, FrozenComponent comp, ref BeforeDamageChangedEvent args)
        {
            args.Damage.DamageDict[comp.FrozenDamage.Id] = 0f;
            args.Damage.TrimZeros();
        }

        private void OnClone(EntityUid _, FrozenComponent comp, ref CloningEvent args)
        {
            var target = args.CloneUid;

            _metaData.SetEntityName(target, comp.BaseName, raiseEvents: true);
            _appearance.SetSkinColor(target, comp.BaseSkinColor);
        }

        private void OnHealthExamine(EntityUid _, FrozenComponent comp, HealthBeingExaminedEvent args)
        {
            args.Message.AddMarkupOrThrow("\n" + Loc.GetString(comp.FrozenHealthString));
        }

        private void OnRejuvenate(EntityUid ent, FrozenComponent comp, RejuvenateEvent args)
        {
            _metaData.SetEntityName(ent, comp.BaseName, raiseEvents: true);
            _appearance.SetSkinColor(ent, comp.BaseSkinColor);

            RemComp<FrozenComponent>(ent);
        }
    }
}
