using Content.Server._WL.Turrets.Components;
using Content.Server.Actions;
using Content.Server.DeviceLinking.Systems;
using Content.Server.DeviceNetwork.Components;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Power.Components;
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

namespace Content.Server._WL.Turrets.Systems
{
    public sealed partial class BuckleableTurretSystem : EntitySystem
    {
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
            SubscribeLocalEvent<TurretMinderConsoleComponent, BoundUIOpenedEvent>(OnUiOpen);

            SubscribeLocalEvent<TurretMinderConsolePressedUiButtonMessage>(OnMessage);

            SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhost);

            //Attempts
            SubscribeLocalEvent<BuckledOnTurretComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<BuckledOnTurretComponent, MoveEvent>(OnMove);
            SubscribeLocalEvent<BuckledOnTurretComponent, StatusEffectAddedEvent>(OnStatusEffectAdded);
            SubscribeLocalEvent<BuckledOnTurretComponent, MobStateChangedEvent>(OnMobStateChanged);

            SubscribeLocalEvent<BuckleableTurretComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            SubscribeLocalEvent<BuckleableTurretComponent, DestructionEventArgs>(OnTerminate);

            SubscribeLocalEvent<TurretMinderConsoleComponent, DestructionEventArgs>(OnConsoleTerminate);
            SubscribeLocalEvent<TurretMinderConsoleComponent, PowerChangedEvent>(OnConsolePowerChanged);
            SubscribeLocalEvent<TurretMinderConsoleComponent, AnchorStateChangedEvent>(OnConsoleAnchorChanged);
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
        private void OnConsoleTerminate(EntityUid console, TurretMinderConsoleComponent comp, DestructionEventArgs args)
            => Unvisit((console, comp));
        private void OnConsolePowerChanged(EntityUid console, TurretMinderConsoleComponent comp, ref PowerChangedEvent args)
        {
            if (!args.Powered)
                Unvisit((console, comp));
        }
        private void OnConsoleAnchorChanged(EntityUid console, TurretMinderConsoleComponent comp, AnchorStateChangedEvent args)
            => Unvisit((console, comp));
        #endregion

        private void OnLink(EntityUid console, TurretMinderConsoleComponent comp, NewLinkEvent args)
        {
            UpdateUiState(console);
        }

        private void OnUiOpen(EntityUid console, TurretMinderConsoleComponent comp, BoundUIOpenedEvent args)
        {
            UpdateUiState(console);
        }

        private void OnMessage(TurretMinderConsolePressedUiButtonMessage args)
        {
            var turret = GetEntity(args.Turret);
            if (!turret.IsValid())
                return;

            var user = args.Actor;

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

        public void Unvisit(BuckleableTurretComponent? comp)
        {
            if (comp?.User == null)
                return;

            RemComp<BuckledOnTurretComponent>(comp.User.Value.Owner);

            var mind = comp.User.Value.Comp.Mind;

            comp.Riding = false;
            comp.User = null;

            if (mind == null)
                return;

            _mind.UnVisit(mind.Value.Owner, mind.Value.Comp);
        }

        public void Unvisit(BuckledOnTurretComponent? comp)
        {
            Unvisit(comp?.Turret?.Comp);
        }

        public void Unvisit(Entity<TurretMinderConsoleComponent>? console, DeviceLinkSourceComponent? comp = null)
        {
            if (console == null)
                return;

            if (!Resolve(console.Value.Owner, ref comp))
                return;

            foreach (var entity in comp.LinkedPorts)
            {
                if (!TryComp<BuckleableTurretComponent>(entity.Key, out var turretComp))
                    continue;

                Unvisit(turretComp);
            }
        }

        public void UpdateUiState(Entity<UserInterfaceComponent?> console, DeviceLinkSourceComponent? devicelinkComp = null)
        {
            if (!Resolve(console.Owner, ref devicelinkComp))
                return;

            var dict = new Dictionary<NetEntity, TurretMinderConsoleBUIStateEntry>();

            foreach (var entity in devicelinkComp.LinkedPorts)
            {
                var ent = entity.Key;

                if (!ent.IsValid())
                    continue;

                if (TerminatingOrDeleted(ent))
                    continue;

                if (!TryComp<BuckleableTurretComponent>(ent, out var comp))
                    continue;

                if (!TryComp<TransformComponent>(ent, out var transformComp))
                    continue;

                if (!TryComp<DeviceNetworkComponent>(ent, out var deviceNetworkComp))
                    continue;

                dict.Add(GetNetEntity(ent), new(
                    comp.Riding || !transformComp.Anchored,
                    deviceNetworkComp.Address,
                    Prototype(ent)?.ID));
            }

            var state = new TurretMinderConsoleBoundUserInterfaceState(dict);

            _ui.SetUiState(console, ConsoleTurretMinderUiKey.Key, state);
        }
    }
}
