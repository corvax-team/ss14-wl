using Content.Server._WL.Destructible.Components;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Humanoid;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.NameModifier.Components;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Enums;

namespace Content.Server._WL.Destructible.Thresholds.Behaviors
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class FrozeBodyBehavior : IThresholdBehavior
    {
        public const float InterpolateStrength = 0.88f;
        public static readonly Color InterpolateColor = Color.CadetBlue;

        public void Execute(EntityUid bodyId, DestructibleSystem system, EntityUid? cause = null)
        {
            var entMan = system.EntityManager;
            var humanoidProfileSys = entMan.System<HumanoidProfileSystem>();
            var transformSys = entMan.System<TransformSystem>();
            var popupSys = entMan.System<SharedPopupSystem>();
            var metaDataSys = entMan.System<MetaDataSystem>();

            var frozenComp = entMan.EnsureComponent<FrozenComponent>(bodyId);

            //Обновляем цвет кожи
            if (!entMan.TryGetComponent<HumanoidProfileComponent>(bodyId, out var humanoidProfileComp))
                return;
            /*

            var curColor = humanoidAppearnceComp.SkinColor;
            frozenComp.BaseSkinColor = curColor;

            humanoidProfileSys.SetSkinColor(
                bodyId,
                Color.InterpolateBetween(curColor, InterpolateColor, InterpolateStrength),
                sync: true,
                verify: false
                );
            */

            //Устанавливаем префикс
            var baseName = Identity.Name(bodyId, entMan);
            frozenComp.BaseName = baseName;

            var genderString = humanoidProfileComp.Gender switch
            {
                Gender.Male => "male",
                Gender.Female => "female",
                _ => "other"
            };

            var newName = $"{Loc.GetString(frozenComp.FrozenPrefix, ("gender", genderString))} {baseName}";

            metaDataSys.SetEntityName(bodyId, newName);

            //Запрещаем хил тела и разрешаем клонирование, убрав компонент гниения
            entMan.RemoveComponent<PerishableComponent>(bodyId);
            entMan.RemoveComponent<InjectableSolutionComponent>(bodyId);

            //Поп-ап
            var msg = Loc.GetString(frozenComp.FrozenPopup,
                ("name", baseName),
                ("gender", genderString));

            popupSys.PopupCoordinates(
                msg,
                transformSys.GetMoverCoordinates(bodyId),
                Robust.Shared.Player.Filter.Pvs(bodyId),
                true,
                PopupType.LargeCaution);
        }
    }
}
