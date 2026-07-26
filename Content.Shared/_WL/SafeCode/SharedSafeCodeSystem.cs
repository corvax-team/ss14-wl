using Content.Shared.Storage;

namespace Content.Shared._WL.SafeCode;

public abstract class SharedSafeCodeSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SafeCodeComponent, StorageInteractAttemptEvent>(OnStorageInteractAttempt);
        SubscribeLocalEvent<SafeCodeComponent, StorageInteractUsingAttemptEvent>(OnStorageUsingAttempt);
    }

    private void OnStorageInteractAttempt(Entity<SafeCodeComponent> ent, ref StorageInteractAttemptEvent args)
    {
        if (ent.Comp.Locked)
            args.Cancelled = true;
    }

    private void OnStorageUsingAttempt(Entity<SafeCodeComponent> ent, ref StorageInteractUsingAttemptEvent args)
    {
        if (ent.Comp.Locked)
            args.Cancelled = true;
    }
}
