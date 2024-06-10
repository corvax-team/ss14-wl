using Content.Server._WL.Turrets.Components;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared._WL.Turrets.Events;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.StatusEffect;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;

namespace Content.Server._WL.Turrets.Systems
{
    public sealed partial class BuckleableTurretSystem : EntitySystem
    {
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly ActionsSystem _actions = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BuckleableTurretComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);
            SubscribeLocalEvent<BuckleableTurretComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<BuckleableTurretComponent, TurretRidingRequestDoAfterEvent>(OnDoAfter);
            SubscribeLocalEvent<BuckleableTurretComponent, TurretExitRidingActionEvent>(OnExitAction);

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

        private void OnVerb(EntityUid turret, BuckleableTurretComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (comp.Riding)
                return;

            if (!TryComp<TransformComponent>(turret, out var xform) || !xform.Anchored)
                return;

            if (HasComp<BuckleableTurretComponent>(args.User))
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, comp.RideTime, new TurretRidingRequestDoAfterEvent(), turret, turret, null)
            {
                BlockDuplicate = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                BreakOnHandChange = true,
                NeedHand = true,
                RequireCanInteract = true,
                DuplicateCondition = DuplicateConditions.SameTarget
            };

            var verb = new AlternativeVerb()
            {
                Act = () =>
                {
                    _doAfter.TryStartDoAfter(doAfterArgs);
                },
                ConfirmationPopup = true,
                IconEntity = GetNetEntity(turret),
                Text = "Управлять",
                Impact = LogImpact.Medium
            };

            args.Verbs.Add(verb);
        }

        private void OnDoAfter(EntityUid turret, BuckleableTurretComponent comp, TurretRidingRequestDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled)
                return;

            var mind = _mind.GetMind(args.User);
            if (mind == null)
                return;

            var buckledComp = EnsureComp<BuckledOnTurretComponent>(args.User);

            buckledComp.Turret = (turret, comp);
            var mindComp = Comp<MindComponent>(mind.Value);
            buckledComp.Mind = (mind.Value, mindComp);

            comp.User = (args.User, buckledComp);
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
            Logger.Debug("1");
            var ent = args.Mind.OwnedEntity;
            if (ent == null)
                return;
            Logger.Debug("2");
            if (!TryComp<BuckledOnTurretComponent>(ent, out var buckledComp))
                return;
            Logger.Debug("3");
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
    }
}
