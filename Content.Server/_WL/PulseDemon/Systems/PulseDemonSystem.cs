using Content.Server.Actions;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.NodeContainer;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.NodeGroups;
using Content.Server._WL.PulseDemon.Components;
using Content.Server.Store.Components;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Maps;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using System.Numerics;
using Robust.Shared.Map;

namespace Content.Server._WL.PulseDemon.Systems;

public sealed partial class PulseDemonSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly PowerNetSystem _power = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IMapManager _map = default!;

    #region WallSpawnOffsets
    private static readonly List<Vector2> Offsets = new()
        {
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(0, -1),
            new Vector2(1, -1),
            new Vector2(-1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1)
        };
    #endregion

    private const string MarkerCablePrototypeID = "MarkerCable";
    private const string HijackAPCsObjective = "HijackAPCsObjective";

    public const string EnergyCurrencyPrototype = "Energy";

    public const int InvisibleWallsLayer = 256;
    public const float WithoutTurfDamageFactor = 10;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(PowerNetSystem));

        SubscribeLocalEvent<PulseDemonComponent, MindAddedMessage>(OnPulseDemonMindGotAdded);

        SubscribeLocalEvent<PulseDemonComponent, ChargeChangedEvent>(OnChargeChange);
        SubscribeLocalEvent<PulseDemonComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<PulseDemonComponent, MoveEvent>(OnMove);

        SubscribeLocalEvent<PulseDemonComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);

        InitializeHijackedComponent();
        InitializeAbilities();
        InitializeShopEventsSubscribers();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PulseDemonComponent, TransformComponent, BatteryComponent>();
        while (query.MoveNext(out var uid, out var pulseDemonComp, out var transform, out var battery))
        {
            if (pulseDemonComp.CanExistOutsideCable)
                continue;

            if (CheckPoweredCablesOnDemon(transform))
            {
                _alerts.ClearAlert(uid, AlertType.WithoutElectricityWarning);
                continue;
            }

            _alerts.ShowAlert(uid, AlertType.WithoutElectricityWarning);

            var factor = transform.Coordinates.GetTileRef(EntityManager, _map) == null
                ? WithoutTurfDamageFactor
                : 1;
            var damageValue = battery.MaxCharge * GetEndurance(pulseDemonComp) / _gameTiming.TickRate * factor;
            DealBatteryDamage(battery, damageValue);
        }
    }

    private void OnPulseDemonMindGotAdded(EntityUid demonUid, PulseDemonComponent component, MindAddedMessage args)
    {
        AddBaseActions(demonUid);
        InitializeComponentFields(demonUid, component);

        if (!TryComp<TransformComponent>(demonUid, out var transformComp))
            return;

        UpdateCablesAroundDemon(transformComp);
        UpdateWallsAroundDemon(transformComp);

        if (!_mind.TryGetMind(demonUid, out var mindId, out var mindComp))
            return;

        _mind.TryAddObjective(mindId, mindComp, HijackAPCsObjective);
    }

    private void OnChargeChange(EntityUid uid, PulseDemonComponent component, ChargeChangedEvent args)
    {
        //Synchronizing the store's balance with the current battery charge of the demon
        if (TryComp<StoreComponent>(uid, out var store))
        {
            store.Balance[EnergyCurrencyPrototype] = args.Charge;
            _store.UpdateUserInterface(uid, uid, store);
        }

        _alerts.ShowAlert(uid, AlertType.Electricity, (short) Math.Clamp(Math.Round(args.Charge / args.MaxCharge * 20), 0, 20));

        if (args.Charge <= 0)
            QueueDel(uid);
    }

    private void OnDamageChange(EntityUid uid, PulseDemonComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

        foreach (var damage in args.DamageDelta.DamageDict)
        {
            var damageValue = damage.Value.Float() * component.DemonDamageModifier;

            if (damage.Key == "Shock")
                damageValue *= -1;

            DealBatteryDamage(battery, damageValue);
        }
    }

    private void OnMove(EntityUid uid, PulseDemonComponent component, MoveEvent args)
    {
        TimeBasedParticlesSpawn(uid, component, args);

        if (component.CanExistOutsideCable)
            return;

        if (!TryComp<TransformComponent>(uid, out var transform))
            return;

        UpdateWallsAroundDemon(transform);
        UpdateWallsWithoutDemonAround();
        UpdateCablesAroundDemon(transform);
        UpdateMarkeredCablesWithoutCables();
    }

    private void OnExamine(EntityUid uid, PulseDemonComponent component, ExaminedEvent args)
    {
        if (args.Examiner == args.Examined)
        {
            args.PushMarkup(Loc.GetString("pulse-demon-levels"), 6);
            args.PushMarkup(Loc.GetString("pulse-demon-absorption-level", ("absorption", component.AbsorptrionLevel)), 5);
            args.PushMarkup(Loc.GetString("pulse-demon-hijackspeed-level", ("hijack", component.HijackSpeedLevel)), 4);
            args.PushMarkup(Loc.GetString("pulse-demon-capacity-level", ("capacity", component.CapacityLevel)), 3);
            args.PushMarkup(Loc.GetString("pulse-demon-endurance-level", ("endurance", component.EnduranceLevel)), 2);
            args.PushMarkup(Loc.GetString("pulse-demon-efficiency-level", ("efficiency", component.EfficiencyLevel)), 1);
            args.PushMarkup(Loc.GetString("pulse-demon-speed-level", ("speed", component.SpeedLevel)), 0);
        }
    }

    private void OnRoundEnd(RoundEndTextAppendEvent args)
    {
        var demons = EntityQuery<PulseDemonComponent, MetaDataComponent, ActorComponent, TransformComponent>();
        if (!demons.Any())
            return;

        foreach (var demon in demons)
        {
            var apcsCount = EntityQuery<ApcComponent, TransformComponent>()
                .Where(apc => apc.Item2.GridUid == demon.Item4.GridUid);

            var hijackedApcsCount = apcsCount.Where(x => HasComp<HijackedByPulseDemonComponent>(x.Item2.Owner)).Count();

            var demonName = demon.Item2.EntityName;
            var username = demon.Item3.PlayerSession.Name;

            var line = Loc.GetString("pulse-demon-round-end", ("demonname", demonName), ("username", username), ("hijackedcount", hijackedApcsCount), ("apcscount", apcsCount.Count()));
            args.AddLine(line);
        }

        args.AddLine($"\n");
    }

    public void DealBatteryDamage(BatteryComponent battery, float damageValue)
    {
        var newChargeValue = battery.CurrentCharge - damageValue;

        _battery.SetCharge(battery.Owner, newChargeValue, battery);
    }

    #region CharacteristicGet
    public static float GetHijackTime(PulseDemonComponent pulseDemonComp)
    {
        return pulseDemonComp.BaseHijackTime / pulseDemonComp.HijackSpeedLevel;
    }

    public static float GetSpeed(PulseDemonComponent pulseDemonComp)
    {
        return pulseDemonComp.BaseSpeed + pulseDemonComp.SpeedLevel * 8f / 100f;
    }

    /// <returns>The time that is spent on a single energy drain.</returns>
    public static float GetEfficiency(PulseDemonComponent pulseDemonComp)
    {
        return Math.Clamp(pulseDemonComp.BaseEfficiency * (0.95f - pulseDemonComp.EfficiencyLevel / 100f), 0.1f, 10f);
    }

    public static float GetAbsorption(PulseDemonComponent pulseDemonComp)
    {
        return pulseDemonComp.BaseAbsorption * pulseDemonComp.AbsorptrionLevel;
    }

    /// <summary>
    /// Responsible for the maximum battery capacity of pulse demon
    /// </summary>
    public static float GetCapacity(PulseDemonComponent pulseDemonComp)
    {
        return pulseDemonComp.BaseCapacity * pulseDemonComp.CapacityLevel;
    }

    /// <summary>
    /// It is necessary to calculate the damage in tick for a demon not standing on the cable.
    /// </summary>
    /// <returns>A decimal number showing how many percent of the maximum energy reserve will be taken away</returns>
    public static float GetEndurance(PulseDemonComponent pulseDemonComp)
    {
        return pulseDemonComp.BaseEndurance / MathF.Sqrt(pulseDemonComp.EnduranceLevel);
    }
    #endregion

    #region PulseDemonUpdate
    public bool CheckPoweredCablesOnDemon(TransformComponent demonTransform)
    {
        var entities = demonTransform.Coordinates.GetEntitiesInTile(LookupFlags.Uncontained, _lookup)
            .Where(HasComp<CableComponent>);

        if (!entities.Any())
            return false;

        foreach (var entity in entities)
        {
            if (!TryComp<NodeContainerComponent>(entity, out var nodeComp))
                continue;

            if (!nodeComp.Nodes.TryGetValue("power", out var node) || node.NodeGroup == null)
                continue;

            var network = (IBasePowerNet) node.NodeGroup;

            var statistic = _power.GetNetworkStatistics(network.NetworkNode);

            if (statistic.InStorageCurrent > 200f)
                return true;
        }

        return false;
    }

    public void UpdateMovementSpeed(PulseDemonComponent pulseDemon)
    {
        var requiredSpeed = GetSpeed(pulseDemon);

        _move.ChangeBaseSpeed(pulseDemon.Owner, requiredSpeed, requiredSpeed, 20);

        _move.RefreshMovementSpeedModifiers(pulseDemon.Owner);
    }

    private void SetCanExistOutsideCableValue(PulseDemonComponent pulseDemonComp, bool value)
    {
        if (pulseDemonComp.CanExistOutsideCable == value)
            return;

        if (!TryComp<FixturesComponent>(pulseDemonComp.Owner, out var fixtures))
            return;

        var fixtureId = fixtures.Fixtures.FirstOrNull();
        if (fixtureId == null)
            return;

        if (value == true)
            _physics.RemoveCollisionMask(pulseDemonComp.Owner, fixtureId.Value.Key, fixtureId.Value.Value, InvisibleWallsLayer);
        else _physics.AddCollisionMask(pulseDemonComp.Owner, fixtureId.Value.Key, fixtureId.Value.Value, InvisibleWallsLayer);

        pulseDemonComp.CanExistOutsideCable = value;
    }
    #endregion

    #region PulseDemonInitialization
    private void InitializeComponentFields(EntityUid demonUid, PulseDemonComponent component)
    {
        var battery = Comp<BatteryComponent>(demonUid);
        _alerts.ShowAlert(demonUid, AlertType.Electricity, (short) Math.Round(battery.CurrentCharge / battery.MaxCharge * 20));

        component.NextParticlesSpawnTime = TimeSpan.FromSeconds(component.ParticlesSpawnInterval);
    }

    private void AddBaseActions(EntityUid demonUid)
    {
        _action.AddAction(demonUid, "PulseDemonActionShop");
        _action.AddAction(demonUid, "PulseDemonActionAbsorption");
        _action.AddAction(demonUid, "PulseDemonActionHijackAPC");
    }
    #endregion

    #region OnMoveActions
    public void UpdateWallsWithoutDemonAround()
    {
        var query = EntityQueryEnumerator<DemonInvisibleWallComponent, TransformComponent>();
        while (query.MoveNext(out var wallUid, out _, out _))
        {
            var entities = _lookup.GetEntitiesInRange(wallUid, 1.44f, LookupFlags.Dynamic)
                .Where(HasComp<PulseDemonComponent>);

            if (entities.Any())
                continue;

            QueueDel(wallUid);
        }
    }

    public void UpdateMarkeredCablesWithoutCables()
    {
        var entities = EntityQueryEnumerator<DemonCableMarkerComponent>();
        while (entities.MoveNext(out var uid, out var demonMarkeredCableComp))
        {
            if (Exists(demonMarkeredCableComp.Entity))
                continue;

            QueueDel(uid);
        }
    }

    private void UpdateCablesAroundDemon(TransformComponent demonTransform)
    {
        if (demonTransform.GridUid == null)
            return;

        var cables = EntityQuery<CableComponent, TransformComponent>()
            .Where(cable => !HasComp<MarkeredCableComponent>(cable.Item1.Owner) && cable.Item2.Coordinates
            .InRange(EntityManager, _transform, demonTransform.Coordinates, 13f));

        foreach (var ent in cables)
        {
            var xForm = Transform(ent.Item1.Owner);
            if (xForm.GridUid == null)
                continue;

            EnsureComp<MarkeredCableComponent>(ent.Item1.Owner);
            var marker = EntityManager.SpawnEntity(MarkerCablePrototypeID, xForm.Coordinates);
            var markeredCable = Comp<DemonCableMarkerComponent>(marker);
            markeredCable.Entity = ent.Item1.Owner;
        }
    }

    private void UpdateWallsAroundDemon(TransformComponent demonTransform)
    {
        foreach (var offset in Offsets)
        {
            var oldPosition = demonTransform.Coordinates;

            var offsetPosition = oldPosition.WithPosition(oldPosition.Position + offset);

            if (offsetPosition.GetTileRef(EntityManager, _map) == null)
                continue;

            var entites = offsetPosition.GetEntitiesInTile(LookupFlags.Uncontained, _lookup)
                .Where(entity => HasComp<DemonInvisibleWallComponent>(entity) || HasComp<CableComponent>(entity));

            if (entites.Any())
                continue;

            EntityManager.SpawnEntity("MarkerWall", offsetPosition);
        }
    }

    private void TimeBasedParticlesSpawn(EntityUid uid, PulseDemonComponent component, MoveEvent args)
    {
        //MoveEvent is also raised if the player turns, but demon cannot turn,
        //So particles would appear if the player clicked around for a long time
        if (args.OldRotation != args.NewRotation && args.NewPosition == args.OldPosition)
            return;

        ///MoveEvent is raised almost every tick,
        ///Plus the error in the form of a delay due to other subscribers of the MoveEvent,
        ///This delay taken into account here <see cref="PulseDemonComponent.ParticlesSpawnInterval"/>
        if (component.NextParticlesSpawnTime > component.CurrentTime)
        {
            component.CurrentTime += _gameTiming.TickPeriod;
            return;
        }
        component.NextParticlesSpawnTime = component.CurrentTime + TimeSpan.FromSeconds(component.ParticlesSpawnInterval);


        var objectsInPulseDemonRange = _lookup.GetEntitiesInRange(uid, component.ParticlesSpawnRadius, LookupFlags.Uncontained);
        foreach (var @object in objectsInPulseDemonRange)
        {
            //ADD ApcPowerProvider, SMES, Battery
            if (Deleted(@object) || !EntityManager.TryGetComponent<ApcPowerReceiverComponent>(@object, out var apcReceiverComp))
                continue;

            if (!apcReceiverComp.Powered)
                continue;

            if (!_random.Prob(component.ParticlesSpawnProbability))
                continue;

            EntityManager.SpawnEntity(_random.Pick(component.ParticlesPrototypes), Transform(@object).Coordinates);
        }
    }
    #endregion
}
