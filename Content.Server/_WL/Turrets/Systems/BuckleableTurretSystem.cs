using Content.Server._WL.Turrets.Components;
using Content.Server.Actions;
using Content.Server.DeviceLinking.Systems;
using Content.Server.DoAfter;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared._WL.Turrets;
using Content.Shared._WL.Turrets.Events;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;
using System.Linq;

namespace Content.Server._WL.Turrets.Systems
{
    public sealed partial class BuckleableTurretSystem : EntitySystem
    {
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly ActionsSystem _actions = default!;
        [Dependency] private readonly UserInterfaceSystem _ui = default!;
        [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BuckleableTurretComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<BuckleableTurretComponent, TurretExitRidingActionEvent>(OnExitAction);

            SubscribeLocalEvent<TurretMinderConsoleComponent, NewLinkEvent>(OnLink);

            SubscribeLocalEvent<TurretMinderConsolePressedUiButtonMessage>(OnMessage);

            //Attempts
            SubscribeLocalEvent<BuckledOnTurretComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<BuckleableTurretComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            SubscribeLocalEvent<BuckledOnTurretComponent, MoveEvent>(OnMove);
            SubscribeLocalEvent<BuckledOnTurretComponent, StatusEffectAddedEvent>(OnStatusEffectAdded);
            SubscribeLocalEvent<BuckledOnTurretComponent, MobStateChangedEvent>(OnMobStateChanged);
            SubscribeLocalEvent<BuckleableTurretComponent, DestructionEventArgs>(OnTerminate);

            SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhost);
        }

        #region Attempts
        private void OnDamageChanged(EntityUid user, BuckledOnTurretComponent comp, DamageChangedEvent args)
        {
            if (!args.DamageIncreased)
                return;

            Unvisit(comp);
        }

        private void OnAnchorChanged(EntityUid user, BuckleableTurretComponent comp, ref AnchorStateChangedEvent args)
            => Unvisit(comp);
        private void OnMove(EntityUid user, BuckledOnTurretComponent comp, ref MoveEvent args)
            => Unvisit(comp);
        private void OnStatusEffectAdded(EntityUid user, BuckledOnTurretComponent comp, ref StatusEffectAddedEvent args)
            => Unvisit(comp);
        private void OnMobStateChanged(EntityUid user, BuckledOnTurretComponent comp, ref MobStateChangedEvent args)
            => Unvisit(comp);
        private void OnTerminate(EntityUid turret, BuckleableTurretComponent comp, DestructionEventArgs args)
            => Unvisit(comp);
        #endregion

        private void OnLink(EntityUid console, TurretMinderConsoleComponent comp, NewLinkEvent args)
        {
            UpdateUiState(console);
        }

        private void OnMessage(TurretMinderConsolePressedUiButtonMessage args)
        {
            var turret = GetEntity(args.Turret);
            var user = GetEntity(args.Entity);

            var comp = EnsureComp<BuckleableTurretComponent>(turret);

            var mind = _mind.GetMind(user);
            if (mind == null)
                return;

            var buckledComp = EnsureComp<BuckledOnTurretComponent>(user);

            buckledComp.Turret = (turret, comp);
            var mindComp = Comp<MindComponent>(mind.Value);
            buckledComp.Mind = (mind.Value, mindComp);

            comp.User = (user, buckledComp);
            comp.Riding = true;

            _mind.Visit(mind.Value, turret, mindComp);
        }

        private void OnMapInit(EntityUid turret, BuckleableTurretComponent comp, MapInitEvent args)
        {
            _actions.AddAction(turret, ref comp.ExitRidingActionContainer, comp.ExitRidingAction);
        }

        private void OnExitAction(EntityUid turret, BuckleableTurretComponent comp, TurretExitRidingActionEvent args)
        {
            Unvisit(comp);
        }

        private void OnGhost(GhostAttemptHandleEvent args)
        {
            var ent = args.Mind.OwnedEntity;
            if (ent == null)
                return;

            if (!TryComp<BuckledOnTurretComponent>(ent, out var buckledComp))
                return;

            Unvisit(buckledComp.Turret?.Comp);
        }

        private void Unvisit(BuckleableTurretComponent? comp)
        {
            if (comp?.User == null)
                return;

            RemComp<BuckledOnTurretComponent>(comp.User.Value.Owner); // '!' потому что тремя строками выше мы уже проверили юзера на null.

            var mind = comp.User.Value.Comp.Mind;

            comp.Riding = false;
            comp.User = null;

            if (mind == null)
                return;

            _mind.UnVisit(mind.Value.Owner, mind.Value.Comp);
        }

        private void Unvisit(BuckledOnTurretComponent? comp)
        {
            Unvisit(comp?.Turret?.Comp);
        }

        public void UpdateUiState(Entity<UserInterfaceComponent?> console, DeviceLinkSourceComponent? devicelinkComp = null)
        {
            if (!Resolve(console.Owner, ref devicelinkComp))
                return;

            var netEntities = devicelinkComp.LinkedPorts
                .Where(x => TryComp<BuckleableTurretComponent>(x.Key, out var turretComp) && !turretComp.Riding)
                .Select(x => GetNetEntity(x.Key));

            var state = new TurretMinderConsoleBoundUserInterfaceState(netEntities);

            _ui.SetUiState(console, ConsoleTurretMinderUiKey.Key, state);
        }
    }
}
