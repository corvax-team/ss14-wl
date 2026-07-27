using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Wallets;

public sealed partial class WalletSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedJobStatusSystem _jobStatus = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WalletComponent, EntInsertedIntoContainerMessage>(OnIdInserted);
        SubscribeLocalEvent<WalletComponent, EntRemovedFromContainerMessage>(OnIdRemoved);
        SubscribeLocalEvent<WalletComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
    }

    private void OnIdInserted(Entity<WalletComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateIdSlot(ent, args.Container);
    }

    private void OnIdRemoved(Entity<WalletComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateIdSlot(ent, args.Container);
    }

    private void OnGetAdditionalAccess(Entity<WalletComponent> ent, ref GetAdditionalAccessEvent args)
    {
        if (ent.Comp.ContainedId is { } id)
            args.Entities.Add(id);

        // Needed for Syndie wallet
        if (!ent.Comp.CopyAccesses || !TryComp<StorageComponent>(ent, out var storage))
            return;

        foreach (var stored in storage.Container.ContainedEntities)
        {
            if (HasComp<IdCardComponent>(stored))
                args.Entities.Add(stored);
        }
    }

    private void UpdateIdSlot(Entity<WalletComponent> ent, BaseContainer container)
    {
        if (container.ID != ent.Comp.IdSlotId)
            return;

        var hasId = container.ContainedEntities.Count > 0;
        ent.Comp.ContainedId = hasId ? container.ContainedEntities[0] : null;
        _appearance.SetData(ent, WalletVisuals.HasId, hasId);
        _jobStatus.UpdateStatus(Transform(ent).ParentUid);

        Dirty(ent);
    }
}

[Serializable, NetSerializable]
public enum WalletVisuals : byte
{
    HasId
}

[Serializable, NetSerializable]
public enum WalletVisualLayers : byte
{
    IdBase,
    IdIcon,
    Frame
}
