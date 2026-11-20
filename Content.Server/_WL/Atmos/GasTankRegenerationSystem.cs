using System.Linq;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Movement.Components;

namespace Content.Server._WL.Atmos;

public sealed partial class GasTankRegenerationSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphere = default!;
    [Dependency] private GasTankSystem _gasTank = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasTankRegenerationComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate,
            after: [typeof(GasTankSystem)]);
    }

    private void OnAtmosUpdate(Entity<GasTankRegenerationComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        if (!TryComp<GasTankComponent>(ent, out var tank) || tank.ReleaseValveOpen ||
            HasComp<ActiveJetpackComponent>(ent))
            return;

        var air = tank.Air;
        // Use the hotter temperature so adding fuel cannot exceed the pressure limit after mixing.
        var temperature = Math.Max(air.Temperature, Atmospherics.T20C);
        var capacity = Math.Min(ent.Comp.MaxMoles,
            ent.Comp.MaxPressure * air.Volume / (Atmospherics.R * temperature));
        var amount = Math.Min(ent.Comp.MolesPerSecond * args.dt, capacity - air.TotalMoles);
        if (amount <= 0)
            return;

        var totalRatio = ent.Comp.GasRatios.Values.Where(ratio => ratio > 0).Sum();
        if (totalRatio <= 0)
            return;

        var fuel = new GasMixture { Temperature = Atmospherics.T20C };
        foreach (var (gas, ratio) in ent.Comp.GasRatios)
        {
            if (ratio > 0)
                fuel.SetMoles(gas, amount * ratio / totalRatio);
        }
        _atmosphere.Merge(air, fuel);
        Dirty(ent.Owner, tank);
        _gasTank.UpdateUserInterface((ent.Owner, tank));
    }
}
