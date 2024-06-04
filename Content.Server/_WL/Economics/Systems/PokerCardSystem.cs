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
        [Dependency] private readonly AppearanceSystem _appearance = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly ContainerSystem _container = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly HandsSystem _hands = default!;

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
                Text = "Перевернуть"
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
                        _popup.PopupCursor("В коробке нет карт!", args.User);
                        return;
                    }

                    var pickedCard = _random.Pick(container.ContainedEntities);
                    if (_container.RemoveEntity(cardBox, pickedCard, containerManagerComp, force: true))
                        _hands.TryPickupAnyHand(args.User, pickedCard, animateUser: true);

                    if (comp.TakePopup)
                        _popup.PopupEntity($"{Identity.Name(args.User, EntityManager)} вытащил карту из колоды.", cardBox);
                },
                IconEntity = GetNetEntity(cardBox),
                Priority = -1,
                Text = "Вытащить случайную карту"
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
                        _popup.PopupCursor("В коробке нет карт!", args.User);
                        return;
                    }

                    foreach (var card in container.ContainedEntities)
                    {
                        FlipCard(card);
                    }
                },
                IconEntity = GetNetEntity(cardBox),
                Priority = -1,
                Text = "Перевернуть все карты в колоде"
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

            if ((bool) dataValue == true)
            {
                _appearance.SetData(card, PokerCardState.IsFlipped, false);
                _metaData.SetEntityName(card, comp.OriginalName);

                if (user != null && comp.FlipPopup)
                    _popup.PopupEntity($"{Identity.Name(user.Value, EntityManager)} вскрыл карту!", card);
            }
            else
            {
                _appearance.SetData(card, PokerCardState.IsFlipped, true);
                _metaData.SetEntityName(card, comp.FlippedCardName);
            }

            _audio.PlayPvs(comp.FlipSound, card, StandartParams);
        }
    }
}
