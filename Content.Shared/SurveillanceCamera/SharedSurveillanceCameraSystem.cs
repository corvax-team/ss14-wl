using Content.Shared.ActionBlocker; //WL-Changes
using Content.Shared.Emp;
using Content.Shared.Item.ItemToggle; //WL-Changes
using Content.Shared.Item.ItemToggle.Components; //WL-Changes
using Content.Shared.SurveillanceCamera.Components;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared.SurveillanceCamera;

public abstract class SharedSurveillanceCameraSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!; //WL-Changes-Включение/выключение

    public override void Initialize()
    {
        SubscribeLocalEvent<SurveillanceCameraComponent, GetVerbsEvent<AlternativeVerb>>(AddVerbs);
        SubscribeLocalEvent<SurveillanceCameraComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<SurveillanceCameraComponent, EmpDisabledRemovedEvent>(OnEmpDisabledRemoved);
        SubscribeLocalEvent<SurveillanceCameraComponent, ItemToggledEvent>(OnToggle);
        SubscribeLocalEvent<SurveillanceCameraComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<SurveillanceCameraComponent, ItemToggleDeactivateAttemptEvent>(OnDeactivateAttempt);
    }

    private void AddVerbs(EntityUid uid, SurveillanceCameraComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanComplexInteract)
            return;

        if (component.NameSet && component.NetworkSet)
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("surveillance-camera-setup"),
            Act = () => OpenSetupInterface(uid, args.User, component)
        };
        args.Verbs.Add(verb);
    }

    private void OnEmpPulse(EntityUid uid, SurveillanceCameraComponent component, ref EmpPulseEvent args)
    {
        if (component.Active)
        {
            args.Affected = true;
            args.Disabled = true;
            SetActive(uid, false);
        }

        //WL-Changes-Включение/выключение-Start
        _itemToggle.TryDeactivate(uid, predicted: false);
        //WL-Changes-Включение/выключение-End
    }

    private void OnEmpDisabledRemoved(EntityUid uid, SurveillanceCameraComponent component, ref EmpDisabledRemovedEvent args)
    {
        if (!HasComp<ItemToggleComponent>(uid))
            SetActive(uid, true);
    }

    // TODO: predict the rest of the server side system
    public virtual void SetActive(EntityUid camera, bool setting, SurveillanceCameraComponent? component = null) { }

    protected virtual void OpenSetupInterface(EntityUid uid, EntityUid player, SurveillanceCameraComponent? camera = null) { }

    #region CorvaxWL
    //WL-Changes-Включение/выключение-Start

    private void OnToggle(Entity<SurveillanceCameraComponent> entity, ref ItemToggledEvent args)
    {
        SetActive(entity, args.Activated);
    }

    private void OnActivateAttempt(Entity<SurveillanceCameraComponent> entity, ref ItemToggleActivateAttemptEvent args)
    {
        if (args.User != null && !_actionBlocker.CanComplexInteract(args.User.Value) || HasComp<EmpDisabledComponent>(entity))
        {
            args.Cancelled = true;
            return;
        }
    }

    private void OnDeactivateAttempt(Entity<SurveillanceCameraComponent> entity, ref ItemToggleDeactivateAttemptEvent args)
    {
        if (args.User != null && !_actionBlocker.CanComplexInteract(args.User.Value) || HasComp<EmpDisabledComponent>(entity))
        {
            args.Cancelled = true;
            return;
        }
    }
    //WL-Changes-Включение/выключение-End
    #endregion
}

[Serializable, NetSerializable]
public enum SurveillanceCameraVisualsKey : byte
{
    Key,
    Layer
}

[Serializable, NetSerializable]
public enum SurveillanceCameraVisuals : byte
{
    Active,
    InUse,
    Disabled,
    // Reserved for future use
    Xray,
    Emp
}
