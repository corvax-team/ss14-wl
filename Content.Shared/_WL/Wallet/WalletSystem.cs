using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Wallet;


public sealed partial class WalletSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedJobStatusSystem _jobStatus = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WalletComponent, EntInsertedIntoContainerMessage>(OnIdInserted);
        SubscribeLocalEvent<WalletComponent, EntRemovedFromContainerMessage>(OnIdRemoved);
    }

    private void OnIdInserted(Entity<WalletComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.IdSlotId)
            return;

        ent.Comp.ContainedId = args.Entity;

        _appearance.SetData(ent, WalletVisuals.HasId, args.Container.ContainedEntities.Count > 0);

        UpdateJobStatus(ent);
    }

    private void OnIdRemoved(Entity<WalletComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.IdSlotId)
            return;

        ent.Comp.ContainedId = null;

        _appearance.SetData(ent, WalletVisuals.HasId, args.Container.ContainedEntities.Count > 0);

        UpdateJobStatus(ent);
    }

    private void UpdateJobStatus(EntityUid uid)
    {
        var parent = Transform(uid).ParentUid;
        _jobStatus.UpdateStatus(parent);
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
