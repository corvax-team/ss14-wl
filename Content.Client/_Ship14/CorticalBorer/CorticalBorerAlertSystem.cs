using Content.Shared.Alert.Components;
using Content.Shared._Ship14.CorticalBorer.Components;

namespace Content.Client._Ship14.CorticalBorer;

public sealed class CorticalBorerAlertSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CorticalBorerComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }

    private void OnGetCounterAmount(Entity<CorticalBorerComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled || ent.Comp.ChemicalsAlert != args.Alert.ID)
            return;

        args.Amount = (int) MathF.Floor(ent.Comp.Chemicals);
    }
}
