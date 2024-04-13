using Content.Server._WL.Damage.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._WL.Damage.Systems;

public sealed partial class FireproofSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ISerializationManager _serMan = default!;

    private const string BurnDamageGroupPrototype = "Burn";

    public override void Initialize()
    {
        SubscribeLocalEvent<FireproofComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FireproofComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FireproofComponent, DamageModifyEvent>(OnDamage);
    }

    private void OnInit(EntityUid uid, FireproofComponent comp, ComponentInit args)
    {
        AddResist(uid, comp);
    }

    private void OnShutdown(EntityUid uid, FireproofComponent comp, ComponentShutdown args)
    {
        RemoveResist(uid, comp);
    }

    private void OnDamage(EntityUid uid, FireproofComponent comp, DamageModifyEvent args)
    {
        var factors = new Dictionary<string, float>();

        var burnGroup = _protoMan.Index<DamageGroupPrototype>(BurnDamageGroupPrototype);
        foreach (var type in burnGroup.DamageTypes)
        {
            factors.Add(type, 0);
        }

        var modifierSet = new DamageModifierSet()
        {
            Coefficients = factors
        };

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifierSet);
    }

    public void AddResist(EntityUid uid, FireproofComponent? fireproofComp = null)
    {
        if (!Resolve(uid, ref fireproofComp))
            return;

        if (TryComp<TemperatureComponent>(uid, out var temperatureComp))
        {
            fireproofComp.HeatDamageThresholdCache = temperatureComp.HeatDamageThreshold;
            temperatureComp.HeatDamageThreshold = float.MaxValue;
        }
        if (TryComp<FlammableComponent>(uid, out var flammableComp))
        {
            _flammable.Extinguish(uid, flammableComp);
            fireproofComp.FirestackFadeCache = flammableComp.FirestackFade;
            flammableComp.FirestackFade = float.MinValue;
            var copy = _serMan.CreateCopy(flammableComp.Damage, notNullableOverride: true);

            fireproofComp.HeatDamageCache = copy;
            flammableComp.Damage = new DamageSpecifier();
        }
    }

    public void RemoveResist(EntityUid uid, FireproofComponent? fireproofComp = null)
    {
        if (!Resolve(uid, ref fireproofComp))
            return;

        if (TryComp<TemperatureComponent>(uid, out var temperatureComp))
        {
            temperatureComp.HeatDamageThreshold = fireproofComp.HeatDamageThresholdCache;
        }
        if (TryComp<FlammableComponent>(uid, out var flammableComp))
        {
            flammableComp.FirestackFade = fireproofComp.FirestackFadeCache;
            flammableComp.Damage = fireproofComp.HeatDamageCache;
        }
    }
}
