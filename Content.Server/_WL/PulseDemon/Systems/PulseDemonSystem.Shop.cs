using Content.Server.Power.EntitySystems;
using Content.Server._WL.PulseDemon.Components;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared._WL.PulseDemon;

namespace Content.Server._WL.PulseDemon.Systems;

public sealed partial class PulseDemonSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    private void InitializeShopEventsSubscribers()
    {
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonShopActionEvent>(OnShopActivate);
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonShopPurchaseEvent>(OnPurchase);
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonShopUpgradeEvent>(OnShopUpgrade);
    }

    private void OnShopActivate(EntityUid uid, PulseDemonComponent component, PulseDemonShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnPurchase(EntityUid uid, PulseDemonComponent component, PulseDemonShopPurchaseEvent args)
    {
        if (TryComp<StoreComponent>(uid, out var store) &&
            store.Balance.TryGetValue(EnergyCurrencyPrototype, out var balance))
        {
            if (balance.Float() == 0f)
            {
                _battery.SetCharge(uid, 1);
                return;
            }

            _battery.SetCharge(uid, balance.Float());
        }
    }

    private void OnShopUpgrade(EntityUid uid, PulseDemonComponent component, PulseDemonShopUpgradeEvent args)
    {
        switch (args.CharacteristicUpgrade?.ToLower())
        {
            case "absorption":
                component.AbsorptrionLevel++;
                break;
            case "hijackspeed":
                component.HijackSpeedLevel++;
                break;
            case "capacity":
                component.CapacityLevel++;
                _battery.SetMaxCharge(uid, GetCapacity(component));
                break;
            case "endurance":
                component.EnduranceLevel++;
                break;
            case "efficiency":
                component.EfficiencyLevel++;
                break;
            case "speed":
                component.SpeedLevel++;
                UpdateMovementSpeed(component);
                break;
            default: return;
        }
    }
}
