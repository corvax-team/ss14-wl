using Content.Shared._WL.Wallet;
using Content.Shared.Access.Components;

namespace Content.Server._WL.Wallet;

public sealed class WalletSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WalletComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
    }

    private void OnGetAdditionalAccess(Entity<WalletComponent> ent, ref GetAdditionalAccessEvent args)
    {
        if (ent.Comp.ContainedId == null)
            return;

        if (HasComp<AccessComponent>(ent.Comp.ContainedId))
            args.Entities.Add(ent.Comp.ContainedId.Value);
    }
}
