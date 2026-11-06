using Content.Shared._WL.Implants;
using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Cloning;
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

namespace Content.Server._WL.Implants;

public sealed partial class ImitatorImplantSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MetaDataSystem _metadata = default!;
    [Dependency] private IdentitySystem _identity = default!;
    [Dependency] private NameModifierSystem _names = default!;
    [Dependency] private SharedScaleVisualsSystem _scale = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private WaggingSystem _wagging = default!;
    [Dependency] private SharedCloningSystem _cloning = default!;
    [Dependency] private ISerializationManager _serialization = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;

    private static readonly ProtoId<CloningSettingsPrototype> IdentitySettings = "ImitatorIdentity";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ImitatorImplantComponent, ImitatorCopyActionEvent>(OnCopy);
        SubscribeLocalEvent<ImitatorImplantComponent, ImitatorCopyDoAfterEvent>(OnCopyCompleted);
        SubscribeLocalEvent<ImitatorImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<ImitatorImplantComponent, ImplantRemovedEvent>(OnRemoved);
        SubscribeLocalEvent<ImitatorImplantComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ImitatorDisguiseComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnCopy(Entity<ImitatorImplantComponent> ent, ref ImitatorCopyActionEvent args)
    {
        if (args.Handled || ent.Comp.CopyDoAfter != null || !CanUse(ent, args.Performer))
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
        return ent.Comp.Image == null &&
               TryComp<SubdermalImplantComponent>(ent, out var implant) && implant.ImplantedEntity == wearer &&
               HasComp<BodyComponent>(wearer) && !HasComp<ImitatorDisguiseComponent>(wearer);
    }

    private bool IsValidTarget(EntityUid target)
    {
        return _mobState.IsDead(target) &&
               TryComp<HumanoidProfileComponent>(target, out var profile) &&
               ProtoMan.TryIndex<SpeciesPrototype>(profile.Species, out var species) && species.RoundStart &&
               HasComp<VisualBodyComponent>(target) && HasComp<BodyComponent>(target);
    }

    private void OnCopyCompleted(Entity<ImitatorImplantComponent> ent, ref ImitatorCopyDoAfterEvent args)
    {
        if (args.Handled || ent.Comp.CopyDoAfter != args.DoAfter.Id)
            return;
        ent.Comp.CopyDoAfter = null;
        if (args.Cancelled || !CanUse(ent, args.User) || args.Target is not { } target || !IsValidTarget(target))
            return;

        var visualSource = target;
        if (TryComp<ImitatorDisguiseComponent>(visualSource, out var existing) && existing.Image is { } existingImage)
            visualSource = existingImage;
        var image = CreateStoredEntity(ent, "imitator-image");
        EnsureComp<BodyComponent>(image);
        EnsureComp<VisualBodyComponent>(image);
        var imageData = EnsureComp<ImitatorImageComponent>(image);
        imageData.Scale = _scale.GetSpriteScale(target);
        if (TryComp<ImitatorImageComponent>(visualSource, out var otherImage))
        {
            imageData.ClothingSpecies = otherImage.ClothingSpecies;
            imageData.Displacements = _serialization.CreateCopy(otherImage.Displacements, notNullableOverride: true);
            imageData.MaleDisplacements = _serialization.CreateCopy(otherImage.MaleDisplacements, notNullableOverride: true);
            imageData.FemaleDisplacements = _serialization.CreateCopy(otherImage.FemaleDisplacements, notNullableOverride: true);
        }
        else if (TryComp<InventoryComponent>(visualSource, out var inventory))
        {
            imageData.ClothingSpecies = inventory.SpeciesId;
            imageData.Displacements = _serialization.CreateCopy(inventory.Displacements, notNullableOverride: true);
            imageData.MaleDisplacements = _serialization.CreateCopy(inventory.MaleDisplacements, notNullableOverride: true);
            imageData.FemaleDisplacements = _serialization.CreateCopy(inventory.FemaleDisplacements, notNullableOverride: true);
        }
        CopyComp(visualSource, image, Comp<HumanoidProfileComponent>(visualSource));
        CopyIdentity(target, image);

        var organs = _containers.GetContainer(image, BodyComponent.ContainerID);
        foreach (var original in Comp<BodyComponent>(visualSource).Organs?.ContainedEntities ?? [])
        {
            if (!HasComp<VisualOrganComponent>(original) && !HasComp<VisualOrganMarkingsComponent>(original))
                continue;

            var organ = Spawn(null);
            CopyIfPresent<OrganComponent>(original, organ);
            CopyIfPresent<VisualOrganComponent>(original, organ);
            CopyIfPresent<VisualOrganMarkingsComponent>(original, organ);
            _containers.Insert(organ, organs);
        }

        ent.Comp.Image = image;
        Apply(ent, args.User);
        HideCopyAction(ent, args.User, Comp<SubdermalImplantComponent>(ent));
        args.Handled = true;
    }

    private EntityUid CreateStoredEntity(EntityUid implant, string containerId)
    {
        var entity = Spawn(null);
        _containers.Insert(entity, _containers.EnsureContainer<ContainerSlot>(implant, containerId));
        return entity;
    }

    private void CopyIfPresent<T>(EntityUid source, EntityUid target) where T : Component
    {
        if (TryComp<T>(source, out var comp))
            CopyComp(source, target, comp);
    }

    private IEnumerable<string> SpeechComponents()
    {
        var names = new HashSet<string>();
        foreach (var species in ProtoMan.EnumeratePrototypes<SpeciesPrototype>())
        {
            if (!species.RoundStart || !ProtoMan.TryIndex(species.Prototype, out var body))
                continue;
            foreach (var (name, entry) in body.Components)
            {
                if (entry.Component is BaseAccentComponent)
                    names.Add(name);
            }
        }
        foreach (var trait in ProtoMan.EnumeratePrototypes<TraitPrototype>())
        {
            if (trait.Category?.Id != "SpeechTraits")
                continue;
#pragma warning disable CS0618
            names.UnionWith(trait.Components.Keys);
#pragma warning restore CS0618
        }
        return names;
    }

    private void CopyIdentity(EntityUid source, EntityUid target)
    {
        var settings = _serialization.CreateCopy(ProtoMan.Index(IdentitySettings), notNullableOverride: true);
        settings.Components.UnionWith(SpeechComponents());
        _cloning.CloneComponents(source, target, settings);
        CopySpeech(source, target);
        _metadata.SetEntityName(target, _names.GetBaseName(source), raiseEvents: false);
        _metadata.SetEntityDescription(target, Description(source));
    }

    private void CopySpeech(EntityUid source, EntityUid target)
    {
        var original = TryComp<SpeechComponent>(source, out var sourceSpeech) ? sourceSpeech : new SpeechComponent();
        var speech = EnsureComp<SpeechComponent>(target);
        speech.SpeechSounds = original.SpeechSounds;
        speech.SpeechVerb = original.SpeechVerb;
        speech.SuffixSpeechVerbs = new(original.SuffixSpeechVerbs);
        speech.AudioParams = original.AudioParams;
        speech.SoundCooldownTime = original.SoundCooldownTime;
        speech.SpeechBubbleOffset = original.SpeechBubbleOffset;
        Dirty(target, speech);
    }

    private void OnImplanted(Entity<ImitatorImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (ent.Comp.Image == null)
            return;
        Apply(ent, args.Implanted);
        HideCopyAction(ent, args.Implanted, Comp<SubdermalImplantComponent>(ent));
    }

    private void HideCopyAction(EntityUid implant, EntityUid wearer, SubdermalImplantComponent comp)
    {
        if (comp.Action is not { } action)
            return;
        _actions.RemoveAction(wearer, action);
        comp.Action = null;
        Dirty(implant, comp);
        QueueDel(action);
    }

    private void Apply(Entity<ImitatorImplantComponent> ent, EntityUid wearer)
    {
        if (ent.Comp.Image is not { } image || HasComp<ImitatorDisguiseComponent>(wearer))
            return;

        if (TryComp<WaggingComponent>(wearer, out var originalWagging))
            _wagging.TryToggleWagging((wearer, originalWagging), false);

        var backup = CreateStoredEntity(ent, "imitator-backup");
        CopyIdentity(wearer, backup);
        CopyIfPresent<ScaleVisualsComponent>(wearer, backup);

        ent.Comp.Backup = backup;
        ent.Comp.Wearer = wearer;
        CopyIdentity(image, wearer);
        var disguise = EnsureComp<ImitatorDisguiseComponent>(wearer);
        disguise.Image = image;
        Dirty(wearer, disguise);
        var imageComp = Comp<ImitatorImageComponent>(image);
        imageComp.Wearer = wearer;
        Dirty(image, imageComp);

        _scale.SetSpriteScale(wearer, imageComp.Scale);
        UpdateInstability((wearer, disguise));
        _identity.QueueIdentityUpdate(wearer);
    }

    private void OnRemoved(Entity<ImitatorImplantComponent> ent, ref ImplantRemovedEvent args) => Restore(ent);

    private void OnShutdown(Entity<ImitatorImplantComponent> ent, ref ComponentShutdown args) => Restore(ent);

    private void Restore(Entity<ImitatorImplantComponent> ent)
    {
        if (ent.Comp.CopyDoAfter is { } pending)
        {
            ent.Comp.CopyDoAfter = null;
            _doAfter.Cancel(pending);
        }
        if (ent.Comp.Wearer is not { } wearer || ent.Comp.Backup is not { } backup)
            return;

        ent.Comp.Wearer = null;
        ent.Comp.Backup = null;
        if (ent.Comp.Image is { } image && TryComp<ImitatorImageComponent>(image, out var stored))
        {
            stored.Wearer = null;
            Dirty(image, stored);
        }

        if (!TerminatingOrDeleted(wearer))
        {
            if (TryComp<WaggingComponent>(wearer, out var wagging))
                _wagging.TryToggleWagging((wearer, wagging), false);
            RemComp<ImitatorDisguiseComponent>(wearer);
            CopyIdentity(backup, wearer);
            if (TryComp<ScaleVisualsComponent>(backup, out var scale))
                _scale.SetSpriteScale(wearer, scale.Scale);
            else
                RemComp<ScaleVisualsComponent>(wearer);
            _identity.QueueIdentityUpdate(wearer);
        }
        QueueDel(backup);
    }

    private void OnDamageChanged(Entity<ImitatorDisguiseComponent> ent, ref DamageChangedEvent args)
        => UpdateInstability(ent);

    private void UpdateInstability(Entity<ImitatorDisguiseComponent> ent)
    {
        var damage = HasComp<DamageableComponent>(ent) ? (float)_damage.GetTotalDamage(ent.Owner) : 0f;
        var instability = Math.Clamp((damage - 90f) / 110f, 0f, 1f) * 0.8f;
        if (ent.Comp.Instability == instability)
            return;
        ent.Comp.Instability = instability;
        Dirty(ent);
    }
}
