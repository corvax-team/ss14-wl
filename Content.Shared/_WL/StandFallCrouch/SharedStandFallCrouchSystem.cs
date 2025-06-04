using Content.Shared._WL.Stand_Fall_Crouch;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Player;
using Content.Shared.Interaction;
using System.Text;
using Content.Shared.Rotation;

namespace Content.Shared._WL.StandFallCrouch
{
    public abstract partial class SharedStandFallCrouchSystem : EntitySystem
    {
        [Dependency] private readonly StandingStateSystem _standingState = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly INetManager _netMan = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        //[Dependency] private readonly AlertsSystem _alertsSystem = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StandFallCrouchComponent, GetVerbsEvent<Verb>>(OnGetInteractionsVerbs);

            SubscribeLocalEvent<StandFallCrouchComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<StandFallCrouchComponent, ComponentShutdown>(OnShutdown);

            SubscribeLocalEvent<StandFallCrouchComponent, StandAttemptEvent>(OnStandAttemp);
            //SubscribeLocalEvent<StandFallCrouchComponent, DownAttemptEvent>(OnDownAttempt);
            SubscribeLocalEvent<StandFallCrouchComponent, ToggleStandCrouchActionEvent>(OnActionPerform);
            SubscribeLocalEvent<StandFallCrouchComponent, StandFallDoAfterEvent>(OnStandFallDoAfter);

            SubscribeLocalEvent<StandFallCrouchComponent, GotUpEvent>(OnGotUp);
            SubscribeLocalEvent<StandFallCrouchComponent, LieDownEvent>(OnLieDown);
            SubscribeLocalEvent<StandFallCrouchComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);

            //SubscribeLocalEvent<StandFallCrouchComponent, BuckledEvent>(OnBuckledEvent);
            SubscribeLocalEvent<StandFallCrouchComponent, UnbuckledEvent>(OnUnbuckledEvent);
        }

        private void OnGetInteractionsVerbs(EntityUid uid, StandFallCrouchComponent component, GetVerbsEvent<Verb> args)
        {
            if (args.Hands == null /*|| args.Using != null*/ || !args.CanAccess || !args.CanInteract || (args.User == args.Target))
                return;

            var attacker = args.User;
            var victum = args.Target;

            if (!CanStandFallAny(victum, attacker))
                return;

            string verbText = "";
            string verbDescription = "";

            SetVerbLocalization(component, out verbText, out verbDescription);

            Verb verb = new()
            {
                Act = () => TryStartStandFallDoAfter(victum, attacker, component, true),
                Impact = Database.LogImpact.Low,
                Text = Loc.GetString(verbText),
                Message = Loc.GetString(verbDescription),
            };

            args.Verbs.Add(verb);
        }

        private void SetVerbLocalization(StandFallCrouchComponent component, out string verbText, out string verbDescription)
        {
            if (!component.IsWantStay)
            {
                verbText = "standfallcrouch-verb-name-standup-other";
                verbDescription = "standfallcrouch-verb-message-standup-other";
            }
            else
            {
                verbText = "standfallcrouch-verb-name-putdown-other";
                verbDescription = "standfallcrouch-verb-message-putdown-other";
            }
        }

        private bool CanStandFallAny(EntityUid victum, EntityUid attacker)
        {
            if (!TryComp<MobStateComponent>(victum, out var mobState))
                return false;

            if (_mobStateSystem.IsAlive(victum, mobState) == false)
                return false;

            //if (victum != attacker && _actionBlockerSystem.CanInteract(victum, null))
            //    return false;

            return true;
        }

        protected void TryStartStandFallDoAfter(EntityUid victum, EntityUid attacker, StandFallCrouchComponent component, bool withPopup = false)
        {
            if (!CanStandFallAny(victum, attacker))
                return;

            float duration = 1f;

            if (victum != attacker)
            {
                if (!component.IsWantStay)
                    duration = component.TimeToStandUpOther;
                else
                    duration = component.TimeToPutDownOther;
            }
            else
            {
                if (!component.IsWantStay)
                    duration = component.TimeToStandUpSelf;
                else
                    duration = component.TimeToPutDownSelf;
            }

            if(withPopup)
                ShowPopup(victum, attacker, component, withPopup);

            var doAfter = new DoAfterArgs(EntityManager, attacker, duration, new StandFallDoAfterEvent(), victum, victum, attacker)
            {
                BreakOnMove = false,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                NeedHand = true,
                Hidden = true
            };

            _doAfterSystem.TryStartDoAfter(doAfter);

        }

        private void ShowPopup(EntityUid victum, EntityUid attacker, StandFallCrouchComponent component, bool withPopup)
        {
            if (!component.IsWantStay)
            {
                //ShowInternalPopup("standfallcrouch-popup-standup-internal", attacker, victum);
                ShowExternalPopup("standfallcrouch-popup-standup-external", attacker, victum);
            }
            else
            {
                //ShowInternalPopup("standfallcrouch-popup-putdown-internal", attacker, victum);
                ShowExternalPopup("standfallcrouch-popup-putdown-external", attacker, victum);
            }

        }

        private void OnStandFallDoAfter(EntityUid uid, StandFallCrouchComponent component, StandFallDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
                return;

            args.Handled = true;

            var attacker = args.User;
            var victim = args.Target.Value;

            if (!CanStandFallAny(victim, attacker))
                return;

            //ToggleStandCrouchActionEvent ev = new ToggleStandCrouchActionEvent();
            //RaiseLocalEvent(victim, ev);

            SetWantStay(victim, !component.IsWantStay, component);
        }

        private void OnActionPerform(EntityUid uid, StandFallCrouchComponent component, ToggleStandCrouchActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;

            TryStartStandFallDoAfter(uid, uid, component);
            //SetWantStay(uid, !component.IsWantStay, component);
        }

        public void SetWantStay(EntityUid entity, bool isWantStay, StandFallCrouchComponent? component = null)
        {
            if (!Resolve(entity, ref component))
                return;

            if (component.IsWantStay == isWantStay)
                return;

            SetWantStayForce(entity, isWantStay, component);
        }

        private void SetWantStayForce(EntityUid entity, bool isWantStay, StandFallCrouchComponent component)
        {
            component.IsWantStay = isWantStay;
            Dirty(entity, component);

            if (!TryComp<RotationVisualsComponent>(entity, out var rotationVisualComp))
                return;

            var xform = Transform(entity);
            var direction = xform.LocalRotation.GetCardinalDir();

            if (component.IsWantStay)
            {
                rotationVisualComp.HorizontalRotation = rotationVisualComp.DefaultRotation;
                Dirty(entity, rotationVisualComp);

                _standingState.Stand(entity);
                if (_netMan.IsServer && _timing.IsFirstTimePredicted)
                {
                    RaiseLocalEvent(entity, new GotUpEvent());
                }
            }
            else
            {
                var angle = direction switch
                {
                    Direction.East or Direction.SouthEast or Direction.NorthEast => Angle.FromDegrees(-rotationVisualComp.DefaultRotation.Degrees),
                    _ => rotationVisualComp.DefaultRotation
                };

                var prevRotation = rotationVisualComp.HorizontalRotation;
                if (prevRotation != angle)
                {
                    rotationVisualComp.HorizontalRotation = angle;
                    Dirty(entity, rotationVisualComp);
                }

                _standingState.Down(entity, dropHeldItems: false);
                if (_netMan.IsServer && _timing.IsFirstTimePredicted)
                {
                    RaiseLocalEvent(entity, new LieDownEvent());
                }
            }

            if (component.StandFallToggleActionEntity != null)
                _actionsSystem.SetToggled(component.StandFallToggleActionEntity, !component.IsWantStay);
        }

        private void ShowInternalPopup(string locString, EntityUid attacker, EntityUid victim)
        {
            _popupSystem.PopupClient(
                Loc.GetString(locString, ("user", attacker), ("target", victim)),
                attacker,
                attacker,
                PopupType.Small
            );
        }
        private void ShowExternalPopup(string locString, EntityUid attacker, EntityUid victim)
        {
            _popupSystem.PopupEntity(
                Loc.GetString(locString, ("user", attacker), ("target", victim)),
                attacker,
                Filter.PvsExcept(attacker),
                true,
                PopupType.Small
                );
        }

        private void OnStandAttemp(EntityUid uid, StandFallCrouchComponent component, StandAttemptEvent args)
        {
            if (component.IsWantStay == false)
                args.Cancel();
        }
        //private void OnDownAttempt(EntityUid uid, StandFallCrouchComponent component, DownAttemptEvent args)
        //{
        //    //if (component.WantStay)
        //    //    args.Cancel();
        //}
        private void OnLieDown(EntityUid uid, StandFallCrouchComponent component, LieDownEvent args)
        {
            //_popupTest.PopupEntity("садится", uid);

            component.IsCrawling = true;

            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            //_alertsSystem.ShowAlert(uid, CrawlingAlert);
            Dirty(uid, component);
        }
        private void OnGotUp(EntityUid uid, StandFallCrouchComponent component, GotUpEvent args)
        {
            //_popupTest.PopupEntity("встает", uid);

            component.IsCrawling = false;

            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            //alertsSystem.ClearAlert(uid, CrawlingAlert);
            Dirty(uid, component);
        }

        private void OnBuckledEvent(EntityUid uid, StandFallCrouchComponent component, BuckledEvent args)
        {
            //if (args.Strap.Comp.Position == StrapPosition.Stand)
            //    SetWantStay(uid, true, component);
            //else if (args.Strap.Comp.Position == StrapPosition.Down)
            //    SetWantStay(uid, false, component);
        }

        private void OnUnbuckledEvent(EntityUid uid, StandFallCrouchComponent component, UnbuckledEvent args)
        {
            if (args.Strap.Comp.Position == StrapPosition.Stand && !component.IsWantStay)
                SetWantStayForce(uid, false, component);
        }


        private void OnRefreshMovespeed(EntityUid uid, StandFallCrouchComponent component, RefreshMovementSpeedModifiersEvent args)
        {
            if(component.IsCrawling)
                args.ModifySpeed(component.WalkModifier, component.SprintModifier);
        }
        private void OnMapInit(EntityUid uid, StandFallCrouchComponent component, MapInitEvent args)
        {
            _actionsSystem.AddAction(uid, ref component.StandFallToggleActionEntity, component.StandFallToggleAction);
        }
        private void OnShutdown(EntityUid uid, StandFallCrouchComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.StandFallToggleActionEntity);
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            //_alertsSystem.ClearAlert(uid, CrawlingAlert);
        }
    }

    public sealed partial class ToggleStandCrouchActionEvent : InstantActionEvent
    {

    }

    public sealed class LieDownEvent
    {

    }

    public sealed class GotUpEvent
    {

    }
}
