using Content.Server._WL.Economics.Components;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._WL.Economics.Visuals;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._WL.Economics.Systems
{
    public sealed partial class PokerCardSystem : EntitySystem
    {
        [Dependency] private AppearanceSystem _appearance = default!;
        [Dependency] private AudioSystem _audio = default!;
        [Dependency] private MetaDataSystem _metaData = default!;
        [Dependency] private PopupSystem _popup = default!;
        [Dependency] private ContainerSystem _container = default!;
        [Dependency] private IRobustRandom _random = default!;
        [Dependency] private HandsSystem _hands = default!;

        public const string CardBoxContainer = "storagebase";
        public static readonly AudioParams StandartParams = new() { Volume = -30 };

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PokerCardComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);
            SubscribeLocalEvent<PokerCardComponent, UseInHandEvent>(OnUseInHand);
            SubscribeLocalEvent<PokerCardComponent, ComponentInit>(OnInit);

            SubscribeLocalEvent<PokerCardContainerComponent, GetVerbsEvent<AlternativeVerb>>(OnContainerVerb);
            SubscribeLocalEvent<PokerCardContainerComponent, ExaminedEvent>(OnCardBoxExamine);
        }

        private void OnVerb(EntityUid card, PokerCardComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            var verb = new AlternativeVerb()
            {
                Act = () => FlipCard(card, args.User, comp),
                IconEntity = GetNetEntity(card),
                Text = Loc.GetString("pokercard-system-flip-verb")
            };

            args.Verbs.Add(verb);
        }

        private void OnUseInHand(EntityUid card, PokerCardComponent comp, UseInHandEvent args)
        {
            FlipCard(card, args.User, comp);
        }

        private void OnInit(EntityUid card, PokerCardComponent comp, ComponentInit _)
        {
            comp.OriginalName = Name(card);
            _appearance.SetData(card, PokerCardState.IsFlipped, true);
            _metaData.SetEntityName(card, comp.FlippedCardName);
        }

        private void OnContainerVerb(EntityUid cardBox, PokerCardContainerComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            var user = args.User;

            var firstVerb = new AlternativeVerb()
            {
                Act = () =>
                {
                    if (!TryComp<ContainerManagerComponent>(cardBox, out var containerManagerComp))
                        return;

                    if (!_container.TryGetContainer(cardBox, CardBoxContainer, out var container, containerManagerComp))
                        return;

                    if (container.ContainedEntities.Count == 0)
                    {
                        _popup.PopupCursor(Loc.GetString("pokercard-system-box-no-cards-popup"), user);
                        return;
                    }

                    var pickedCard = _random.Pick(container.ContainedEntities);
                    if (_container.RemoveEntity(cardBox, pickedCard, containerManagerComp, force: true))
                        _hands.TryPickupAnyHand(user, pickedCard, animateUser: true);

                    if (comp.TakePopup)
                        _popup.PopupEntity(Loc.GetString("pokercard-system-get-card-popup", ("name", Identity.Name(user, EntityManager)), ("ent", user)), cardBox);
                },
                IconEntity = GetNetEntity(cardBox),
                Priority = -1,
                Text = Loc.GetString("pokercard-system-get-random-card-verb")
            };

            var secondVerb = new AlternativeVerb()
            {
                Act = () =>
                {
                    if (!TryComp<ContainerManagerComponent>(cardBox, out var containerManagerComp))
                        return;

                    if (!_container.TryGetContainer(cardBox, CardBoxContainer, out var container, containerManagerComp))
                        return;

                    if (container.ContainedEntities.Count == 0)
                    {
                        _popup.PopupCursor(Loc.GetString("pokercard-system-box-no-cards-popup"), user);
                        return;
                    }

                    foreach (var card in container.ContainedEntities)
                    {
                        FlipCard(card);
                    }
                },
                IconEntity = GetNetEntity(cardBox),
                Priority = -1,
                Text = Loc.GetString("pokercard-system-flip-all-cards-verb")
            };

            args.Verbs.Add(firstVerb);
            args.Verbs.Add(secondVerb);
        }

        private void OnCardBoxExamine(EntityUid cardBox, PokerCardContainerComponent comp, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            if (!_container.TryGetContainer(cardBox, CardBoxContainer, out var container))
                return;

            args.PushMarkup(Loc.GetString("economics-card-box-remaining-cards", ("amount", container.ContainedEntities.Count)));
        }

        public void FlipCard(EntityUid card, EntityUid? user = null, PokerCardComponent? comp = null)
        {
            if (!Resolve(card, ref comp))
                return;

            if (!_appearance.TryGetData(card, PokerCardState.IsFlipped, out var dataValue))
                return;

            if ((bool)dataValue == true)
            {
                _appearance.SetData(card, PokerCardState.IsFlipped, false);
                _metaData.SetEntityName(card, comp.OriginalName);

                if (user != null && comp.FlipPopup)
                    _popup.PopupEntity(Loc.GetString("pokercard-system-reveal-card-popup", ("name", Identity.Name(user.Value, EntityManager)), ("ent", user.Value)), card);
            }
            else
            {
                _appearance.SetData(card, PokerCardState.IsFlipped, true);
                _metaData.SetEntityName(card, Loc.GetString(comp.FlippedCardName));
            }

            _audio.PlayPvs(comp.FlipSound, card, StandartParams);
        }
    }
}
