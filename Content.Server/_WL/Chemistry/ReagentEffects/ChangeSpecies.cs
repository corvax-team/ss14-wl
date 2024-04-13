using Content.Server.Humanoid;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Corvax.TTS;
using Content.Shared.Humanoid;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System.Linq;

namespace Content.Server._WL.Chemistry.ReagentEffects
{
    public sealed partial class ChangeSpecies : ReagentEffect
    {
        [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<PolymorphPrototype>))]
        public string PolymorphPrototype { get; set; }

        public override void Effect(ReagentEffectArgs args)
        {
            var entityManager = args.EntityManager;
            var _polymorph = entityManager.System<PolymorphSystem>();
            var _humanoidAppearance = entityManager.System<HumanoidAppearanceSystem>();

            if (!entityManager.TryGetComponent<HumanoidAppearanceComponent>(args.SolutionEntity, out var humanoidAppearanceComp) ||
                !entityManager.TryGetComponent<TTSComponent>(args.SolutionEntity, out var ttsComp))
                return;

            var skinColor = humanoidAppearanceComp.SkinColor;
            var tts = ttsComp.VoicePrototypeId ?? "";
            var sex/*despair*/ = humanoidAppearanceComp.Sex;
            var eyeColor = humanoidAppearanceComp.EyeColor;

            entityManager.EnsureComponent<PolymorphableComponent>(args.SolutionEntity);
            var newEntity = _polymorph.PolymorphEntity(args.SolutionEntity, PolymorphPrototype);
            if (newEntity == null)
                return;

            if (!entityManager.TryGetComponent<HumanoidAppearanceComponent>(newEntity, out var newAppearanceComp))
                return;

            _humanoidAppearance.SetSex(newEntity.Value, sex, true);
            _humanoidAppearance.SetSkinColor(newEntity.Value, skinColor, true, false);
            _humanoidAppearance.SetTTSVoice(newEntity.Value, tts, newAppearanceComp);
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-change-species",
                ("chance", Probability),
                ("species", prototype.Index<EntityPrototype>(prototype.Index<PolymorphPrototype>(PolymorphPrototype).Configuration.Entity.Id).Components.Values
                .Select(comp =>
                {
                    if (comp.Component is HumanoidAppearanceComponent humAppearanceComp)
                        return prototype.Index(humAppearanceComp.Species).Name;

                    return null;
                }).FirstOrDefault() ?? ""));
    }
}
