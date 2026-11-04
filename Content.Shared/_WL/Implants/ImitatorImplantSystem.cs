using Content.Shared._WL.Implants;
using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Cloning;
using Content.Shared.Changeling.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Sprite;
using Content.Shared.Traits;
using Content.Shared.Wagging;
using Content.Shared.Speech.Components;
using Content.Shared.Speech;
using Content.Shared.Inventory;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Implants;

public sealed partial class ImitatorImplantSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MetaDataSystem _metadata = default!;
    [Dependency] private IdentitySystem _identity = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedCloningSystem _cloning = default!;
    [Dependency] private ISerializationManager _serialization = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedVisualBodySystem _visualBody = default!;
    [Dependency] private SharedChangelingIdentitySystem _changelingIdentity = default!;

    [SubscribeLocalEvent]
    private void OnCopy(Entity<ImitatorImplantComponent> ent, ref ImitatorCopyActionEvent args)
    {
        if (args.Handled || ent.Comp.CopyDoAfter is not null || !CanUse(ent, args.Performer) || ent.Comp.Used)
            return;

        if (!IsValidTarget(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("imitator-invalid-target"), args.Performer, args.Performer);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, TimeSpan.FromSeconds(10),
            new ImitatorCopyDoAfterEvent(), ent, target: args.Target, used: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DistanceThreshold = 2f,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter, out ent.Comp.CopyDoAfter);
        if (!args.Handled)
            ent.Comp.CopyDoAfter = null;
    }

    private bool CanUse(Entity<ImitatorImplantComponent> ent, EntityUid wearer)
    {
        return ent.Comp.Image is null &&
               TryComp<SubdermalImplantComponent>(ent, out var implant) &&
               implant.ImplantedEntity == wearer;
    }

    private bool IsValidTarget(EntityUid target)
    {
        return _mobState.IsDead(target) &&
               TryComp<HumanoidProfileComponent>(target, out var profile) &&
               ProtoMan.TryIndex<SpeciesPrototype>(profile.Species, out var species) && species.RoundStart &&
               HasComp<VisualBodyComponent>(target) && HasComp<BodyComponent>(target);
    }

    [SubscribeLocalEvent]
    private void OnCopyCompleted(Entity<ImitatorImplantComponent> ent, ref ImitatorCopyDoAfterEvent args)
    {
        if (args.Handled || ent.Comp.CopyDoAfter != args.DoAfter.Id)
            return;

        ent.Comp.CopyDoAfter = null;

        if (args.Cancelled || !CanUse(ent, args.User) || args.Target is not { } target || !IsValidTarget(target))
            return;

        var image = _changelingIdentity.CloneToPausedMap(ent.Comp.Settings, target);

        ent.Comp.Image = image;
        Apply(ent, args.User);
        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnImplanted(Entity<ImitatorImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (ent.Comp.Image == null)
            return;

        Apply(ent, args.Implanted);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<ImitatorImplantComponent> ent, ref ImplantRemovedEvent args) => Restore(ent);

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<ImitatorImplantComponent> ent, ref ComponentShutdown args)
    {
        Restore(ent);

        QueueDel(ent.Comp.Image);
    }

    [SubscribeLocalEvent]
    private void OnDamageChanged(Entity<ImitatorDisguiseComponent> ent, ref DamageChangedEvent args) => UpdateInstability(ent);

    private void HideCopyAction(EntityUid implant, SubdermalImplantComponent? comp = null)
    {
        if (!Resolve(implant, ref comp) ||
                comp.ImplantedEntity is not {} wearer ||
                comp.Action is not { } action)
            return;

        _actions.RemoveAction(wearer, action);
        comp.Action = null;

        Dirty(implant, comp);
    }

    private void CopyTo(EntityUid source, EntityUid target, ProtoId<CloningSettingsPrototype>? settings = null)
    {
        if (settings is {} settingsId &&
                ProtoMan.TryIndex<CloningSettingsPrototype>(settingsId, out var settingsProto))
            _cloning.CloneComponents(source, target, settingsProto);

        _visualBody.CopyAppearanceFrom(source, target);

        _metadata.SetEntityName(target, Name(source), raiseEvents: false);
        _metadata.SetEntityDescription(target, Description(source));

        if (TryComp<InventoryComponent>(source, out var sourceInventory) &&
                TryComp<InventoryComponent>(target, out var targetInventory))
        {
            targetInventory.Displacements = _serialization.CreateCopy(sourceInventory.Displacements, notNullableOverride: true);
            targetInventory.MaleDisplacements = _serialization.CreateCopy(sourceInventory.MaleDisplacements, notNullableOverride: true);
            targetInventory.FemaleDisplacements = _serialization.CreateCopy(sourceInventory.FemaleDisplacements, notNullableOverride: true);

            Dirty(target, targetInventory);
        }

        _identity.QueueIdentityUpdate(target);
    }

    private void Apply(Entity<ImitatorImplantComponent> ent, EntityUid wearer)
    {
        if (ent.Comp.Image is not { } image)
            return;

        if (_changelingIdentity.CloneToPausedMap(ent.Comp.Settings, wearer) is not {} backup)
            return;

        ent.Comp.Backup = backup;
        ent.Comp.Used = true;

        CopyTo(wearer, backup);

        CopyTo(image, wearer, ent.Comp.Settings);

        var disguise = EnsureComp<ImitatorDisguiseComponent>(wearer);
        UpdateInstability((ent, disguise));

        HideCopyAction(ent);
    }

    private void Restore(Entity<ImitatorImplantComponent> ent)
    {
        if (ent.Comp.CopyDoAfter is { } pending)
        {
            ent.Comp.CopyDoAfter = null;
            _doAfter.Cancel(pending);
        }

        if (!TryComp<SubdermalImplantComponent>(ent, out var subdermal) ||
                subdermal.ImplantedEntity is not { } wearer ||
                ent.Comp.Backup is not { } backup)
            return;

        ent.Comp.Backup = null;

        if (!TerminatingOrDeleted(wearer))
        {
            CopyTo(backup, wearer, ent.Comp.Settings);
            RemComp<ImitatorDisguiseComponent>(wearer);
        }

        QueueDel(backup);
    }

    private void UpdateInstability(Entity<ImitatorDisguiseComponent> ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
        {
            ent.Comp.Instability = 0;
        }
        else
        {
            var damage = (float)_damage.GetPositiveDamage((ent, damageable)).GetTotal();
            ent.Comp.Instability = Math.Clamp((damage - 90f) / 110f, 0f, 1f) * 0.8f;
        }

        Dirty(ent, ent.Comp);
    }
}
