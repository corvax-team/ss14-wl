using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Humanoid;
using Content.Server.Mind;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Speech;
using Content.Server.Stunnable;
using Content.Shared._WL.Slimes;
using Content.Shared._WL.Slimes.Enums;
using Content.Shared._WL.Slimes.Events;
using Content.Shared._WL.Slimes.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.NameIdentifier;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Physics;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using System.Numerics;

namespace Content.Server._WL.Slimes.Systems;

public sealed partial class SlimeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly NPCSystem _npc = default!;


    [ValidatePrototypeId<DamageTypePrototype>]
    private const string CausticDamageTypePrototype = "Caustic";

    [ValidatePrototypeId<EntityPrototype>]
    private const string SlimeEatActionPrototype = "ActionSlimeEat";

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string SlimeAngryFaction = "SlimeAngry";

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string SlimeFriendFaction = "SlimeFriend";

    private const float SlimeVisionRange = 7.5f;

    /// <summary>
    /// A purely conditional value is needed to limit some functions depending on the relationship with the slime.
    /// </summary>
    private const int SlimeFriendRelationshipPoints = 90;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, SlimeChangeRelationshipEvent>(OnRelationshipPointsChange);

        SubscribeLocalEvent<SlimeComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<SlimeComponent, SlimeLifeStageChangeEvent>(OnLifeStageChange);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<SlimeComponent, DamageChangedEvent>(OnDamageChanged);

        SubscribeLocalEvent<SlimeComponent, ListenEvent>(OnCommand);

        SubscribeLocalEvent<SlimeComponent, SlimeEatingActionEvent>(OnEatActionEvent);
        SubscribeLocalEvent<SlimeComponent, SlimeEatingDoAfterEvent>(OnEatDoAfter);
    }

    //TODO make a separate timer for the action launch and for commands
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlimeComponent, HungerComponent, MobStateComponent, ActionsComponent>();
        while (query.MoveNext(out var uid, out var slimeComp, out var hungerComp, out var mobState, out var actions))
        {
            //Dead neither grow, nor feed, nor kill
            if (mobState.CurrentState is MobState.Dead)
                continue;

            if (slimeComp.LastTime > slimeComp.GrowCheckInterval)
            {
                //Manki eating
                if (_random.Prob(slimeComp.EatProbability) &&
                    !HasComp<ActorComponent>(uid) &&
                    !slimeComp.IsEating &&
                    slimeComp.EatActionContainer != null)
                {
                    var target = _lookup.GetEntitiesInRange(uid, 1.44f, LookupFlags.Dynamic)
                        .FirstOrNull(entity => slimeComp.TastePreferences.IsValid(entity));

                    if (target != null)
                    {
                        var actionData = Comp<EntityTargetActionComponent>(slimeComp.EatActionContainer.Value);
                        var eventData = new SlimeEatingActionEvent()
                        {
                            Performer = uid,
                            Target = target.Value
                        };

                        _actions.PerformAction(uid, actions, slimeComp.EatActionContainer.Value, actionData, eventData, _gameTiming.CurTime);
                    }
                }

                //Grow
                if (hungerComp.CurrentThreshold is HungerThreshold.Okay or HungerThreshold.Overfed &&
                    _random.Prob(slimeComp.GrowProbability))
                {
                    slimeComp.GrowthStage++;

                    var growthData = slimeComp.GrowthData[slimeComp.CurrentAge];

                    if (slimeComp.GrowthStage >= growthData.GrowthStageBound)
                    {
                        RaiseLocalEvent(uid, new SlimeLifeStageChangeEvent(uid, slimeComp.CurrentAge + 1, slimeComp.CurrentAge));
                        slimeComp.GrowthStage = 0;
                    }
                }

                //Anger
                if (TryComp<NpcFactionMemberComponent>(uid, out var npcFactionComp))
                {
                    if (slimeComp.CanBeAngry && hungerComp.CurrentThreshold is HungerThreshold.Dead or HungerThreshold.Starving)
                        _npcFaction.AddFaction((uid, npcFactionComp), SlimeAngryFaction, true);
                    else _npcFaction.RemoveFaction((uid, npcFactionComp), SlimeAngryFaction);
                }

                //Check command
                CheckCommandOnSlime(slimeComp);

                slimeComp.LastTime = TimeSpan.Zero;
            }
            else slimeComp.LastTime += _gameTiming.TickPeriod;

            SetSlimeFaceAppearanceData(uid, hungerComp.CurrentThreshold);
        }
    }

    private void OnRelationshipPointsChange(EntityUid uid, SlimeComponent comp, SlimeChangeRelationshipEvent args)
    {
        if (!TryComp<NpcFactionMemberComponent>(args.Target, out var factionComp))
            return;

        if (args.NewPoints >= SlimeFriendRelationshipPoints)
            _npcFaction.AddFaction((args.Target, factionComp), SlimeFriendFaction, true);
        else _npcFaction.RemoveFaction((args.Target, factionComp), SlimeFriendFaction, true);
    }

    private void OnInit(EntityUid uid, SlimeComponent comp, ComponentInit args)
    {
        SetGrowStage(uid, comp.CurrentAge, comp, true, true, true, true);

        comp.CurrentMutationProbability = _random.NextFloat(comp.MinMutationProbabilityBound, comp.MaxMutationProbabilityBound);
        comp.EatActionContainer = _actions.AddAction(uid, SlimeEatActionPrototype);
    }

    private void OnLifeStageChange(EntityUid uid, SlimeComponent comp, SlimeLifeStageChangeEvent args)
    {
        SetGrowStage(uid, args.NewStage, comp);

        if (comp.CurrentAge is SlimeLifeStage.Humanoid)
        {
            if (comp.CanBecomeHumanoid)
                Humanoidization(uid, comp);

            else Split(uid, comp);
        }
    }

    /// <summary>
    /// It is necessary for the correct display of the sprite of the dead slime. I'm too lazy to make my own visualizer.
    /// </summary>
    private void OnMobStateChange(EntityUid uid, SlimeComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead)
            SetGrowStage(uid, SlimeLifeStage.Dead, comp);
    }

    private void OnDamageChanged(EntityUid uid, SlimeComponent comp, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        if (args.Origin is null)
            return;

        //You shouldn't beat up someone you want to be friends with...
        if (comp.Relationships.TryGetValue(args.Origin.Value, out var value))
            comp.Relationships[args.Origin.Value] = Math.Clamp(value - 50/*infinity*/, 0, int.MaxValue);
    }

    private void OnCommand(EntityUid uid, SlimeComponent comp, ListenEvent args)
    {
        if (HasComp<ActorComponent>(uid))
            return;

        //Slime does not follow the commands of non-friends
        if (!comp.Relationships.TryGetValue(args.Source, out var relationPoints))
            return;

        if (TryComp<NameIdentifierComponent>(uid, out var slimeNumber))
        {
            if (!args.Message.Contains(slimeNumber.Identifier.ToString()))
                return;
        }

        var prototypes = _protoMan.EnumeratePrototypes<SlimeCommandPrototype>();
        foreach (var prototype in prototypes)
        {
            if (!prototype.KeyWords.Any(keyword => args.Message.Contains(keyword, StringComparison.CurrentCultureIgnoreCase)))
                continue;

            if (!SetCommand(uid, args.Source, prototype, comp, args.Message, relationPoints))
                continue;

            break;
        }
    }

    private void OnEatActionEvent(EntityUid uid, SlimeComponent comp, SlimeEatingActionEvent args)
    {
        if (args.Handled)
            return;

        if (MakeSlimeToEatTarget(uid, args.Target, 5, comp))
            args.Handled = true;

        args.Handled = false;
    }

    private void OnEatDoAfter(EntityUid uid, SlimeComponent comp, SlimeEatingDoAfterEvent args)
    {
        if (args.Target == null)
            return;

        var slimeCoords = Transform(uid).Coordinates;

        if (args.Handled || args.Cancelled || !IsDoAfterAllowed(uid, args.Target.Value))
        {
            MakeSlimeToLeaveTarget(uid, comp);
            return;
        }

        //Eating
        var growthData = comp.GrowthData[comp.CurrentAge];

        var damageType = _protoMan.Index<DamageTypePrototype>(CausticDamageTypePrototype);
        var damage = new DamageSpecifier(damageType, growthData.EatingDamage);

        _damageable.TryChangeDamage(args.Target, damage, true, false);
        _hunger.ModifyHunger(uid, growthData.HungerSupply);

        //Building relationships
        var playersAround = EntityQueryEnumerator<ActorComponent, MobStateComponent>();
        while (playersAround.MoveNext(out var playerUid, out _, out _))
        {
            var targetCoords = Transform(playerUid).Coordinates;
            if (!targetCoords.InRange(EntityManager, _transform, slimeCoords, SlimeVisionRange))
                continue;

            ChangeSlimeRelationship(uid, playerUid, 1, comp);
        }

        args.Repeat = true;
    }

    /// <summary>
    /// Sets the command to be executed after the expiration.
    /// </summary>
    /// <param name="slime">Slime who will execute the command</param>
    /// <param name="source">Initiator of the message</param>
    /// <param name="commandPrototype"></param>
    /// <param name="slimeComp"></param>
    /// <param name="checkRelationship">If it has a value, it will be compared with the boundaries in <paramref name="commandPrototype"/>.</param>
    /// <returns>True if the command was executed successfully, false if not.</returns>
    public bool SetCommand(EntityUid slime, EntityUid source, SlimeCommandPrototype commandPrototype, SlimeComponent? slimeComp = null, string? chatMessage = null, int? checkRelationship = null)
    {
        if (!Resolve(slime, ref slimeComp, false))
            return false;

        if (slime == source)
            return false;

        if (checkRelationship != null && (checkRelationship < commandPrototype.MinRelationshipPoints ||
            checkRelationship > commandPrototype.MaxRelationshipPoints))
            return false;

        var commandArgs = new SlimeCommandArgs(slime, source, commandPrototype.KeyWords.ToArray(), chatMessage, EntityManager);
        slimeComp.CommandToCommit = (commandPrototype.Command, commandArgs);

        return true;
    }

    /// <summary>
    /// Turns slime into a humanoid, does not check the stage of growth.
    /// </summary>
    /// <param name="slime"></param>
    public void Humanoidization(EntityUid slime, SlimeComponent? slimeComponent = null)
    {
        if (!Resolve(slime, ref slimeComponent, false))
            return;

        var slimeCoords = Transform(slime).Coordinates;
        var humanoidSlime = EntityManager.SpawnEntity(slimeComponent.HumanoidPrototype, slimeCoords);

        _humanoidAppearance.SetSkinColor(humanoidSlime, slimeComponent.HumanoidStageSkinColor);

        if (_mind.TryGetMind(slime, out var mindId, out var mindComp))
            _mind.TransferTo(mindId, humanoidSlime, mind: mindComp);

        _metaData.SetEntityName(humanoidSlime, MetaData(slime).EntityName);

        var ghostRole = EnsureComp<GhostRoleComponent>(humanoidSlime);
        ghostRole.RoleDescription = Loc.GetString("slime-humanoid-ghost-role-desc");
        ghostRole.RoleName = Name(humanoidSlime);

        EnsureComp<GhostTakeoverAvailableComponent>(humanoidSlime);

        QueueDel(slime);
    }

    /// <summary>
    /// Splits slime into lower forms.
    /// </summary>
    /// <param name="slime"></param>
    /// <param name="slimeComponent"></param>
    public void Split(EntityUid slime, SlimeComponent? slimeComponent = null)
    {
        //TODO weight-based mutation choose

        if (!Resolve(slime, ref slimeComponent, false))
            return;

        var mutationPrototype = _protoMan.EnumeratePrototypes<SlimeMutationPrototype>()
            .Where(proto => proto.SlimeGroup.Equals(slimeComponent.SlimeGroupName, StringComparison.CurrentCultureIgnoreCase))
            .FirstOrDefault();

        var splittingSlimePrototype = Prototype(slime)?.ID;
        if (splittingSlimePrototype is null)
            return;

        var splittingSlimeCoords = Transform(slime).Coordinates;
        var splittingGrowthData = slimeComponent.GrowthData[slimeComponent.CurrentAge];

        //Summon slimes
        var summonPoints = 0;
        while (true)
        {
            var stage = (SlimeLifeStage) _random.Next(1, (int) slimeComponent.CurrentAge);

            string chosenMutation;
            if (_random.Prob(slimeComponent.CurrentMutationProbability) && mutationPrototype is not null)
            {
                chosenMutation = mutationPrototype.GetMutation(slime, _random, EntityManager);
            }
            else
            {
                chosenMutation = splittingSlimePrototype;
            }

            var spawnedSlime = SpawnNearby(splittingSlimeCoords, chosenMutation);
            SetGrowStage(spawnedSlime, stage);

            var spawnedSlimeComp = Comp<SlimeComponent>(spawnedSlime);

            if (slimeComponent.PassMutationPropertiesNextGeneration.PassMutationProbability)
            {
                spawnedSlimeComp.CurrentMutationProbability = slimeComponent.CurrentMutationProbability;
            }
            if (slimeComponent.PassMutationPropertiesNextGeneration.Recursive)
            {
                spawnedSlimeComp.PassMutationPropertiesNextGeneration.Recursive = true;
            }

            summonPoints += spawnedSlimeComp.GrowthData[stage].SplitCost;
            if (summonPoints > splittingGrowthData.SplitPointsAmount)
            {
                QueueDel(spawnedSlime);
                break;
            }
        }

        QueueDel(slime);
    }

    /// <summary>
    /// Sets the growth stage for a specific slime.
    /// </summary>
    /// <param name="slime"></param>
    /// <param name="stage"></param>
    /// <param name="slimeComponent"></param>
    /// <param name="addPrefix">Whether to add a prefix before the name. For example: ancient grey slime.</param>
    /// <param name="addSpeedDebuff"></param>
    /// <param name="addAttackBuffs"></param>
    /// <param name="isShapeIncrease">Whether to change the fixture shape of the slime.</param>
    /// <remarks>If the stage is Dead, then slime will just die.</remarks>
    public void SetGrowStage(EntityUid slime,
        SlimeLifeStage stage,
        SlimeComponent? slimeComponent = null,
        bool addPrefix = true,
        bool addSpeedDebuff = true,
        bool addAttackBuffs = true,
        bool isShapeIncrease = true)
    {
        if (!Resolve(slime, ref slimeComponent, false))
            return;

        slimeComponent.CurrentAge = stage;

        //Prefix
        if (addPrefix && TryComp<MetaDataComponent>(slime, out var metadata))
        {
            var prefix = Enum.GetName(stage)?.ToString().ToLower();
            var name = metadata.EntityPrototype?.Name;
            var identifier = "";
            if (TryComp<NameIdentifierComponent>(slime, out var nameIdentifierComp))
                identifier = nameIdentifierComp.FullIdentifier.ToString();

            //shit
            if (name != null && prefix != null)
                _metaData.SetEntityName(slime, (GetLocLifeStage(slimeComponent.CurrentAge).ToLower() + " " + name + " " + identifier).TrimEnd(), metadata);
        }

        if (stage is SlimeLifeStage.Humanoid)
            return;

        //Sprite
        SetSlimeBodyAppearanceData(slime, stage);

        if (stage is SlimeLifeStage.Dead)
            return;

        //Growth data
        if (!slimeComponent.GrowthData.TryGetValue(slimeComponent.CurrentAge, out var growthData))
            return;

        //Cores Amount
        var growthCoreAmount = growthData.CoreAmount;

        slimeComponent.CoreAmount = Math.Max(slimeComponent.CoreAmount, growthCoreAmount);

        //Debuffs
        if (addSpeedDebuff && TryComp<MovementSpeedModifierComponent>(slime, out var movementComp))
            _movement.ChangeBaseSpeed(slime, growthData.WalkSpeed, growthData.SprintSpeed, 20f, movementComp);

        if (addAttackBuffs && TryComp<MeleeWeaponComponent>(slime, out var meleeWeapon))
        {
            meleeWeapon.AttackRate = growthData.AttackRate;
            meleeWeapon.ClickDamageModifier = (growthData.Damage / meleeWeapon.Damage.GetTotal());
        }

        if (isShapeIncrease && TryComp<FixturesComponent>(slime, out var fixtures))
        {
            var fixture = fixtures.Fixtures.First();
            _physics.SetRadius(slime, "fix1"/*no shit*/, fixture.Value, fixture.Value.Shape, growthData.FixtureShape);
        }
    }

    /// <summary>
    /// Makes specific slime eat specific target.
    /// </summary>
    /// <param name="slime"></param>
    /// <param name="target"></param>
    /// <param name="targetStunTime"></param>>
    /// <param name="slimeComp"></param>
    /// <remarks>DOES NOT CHECK the distance between the target and the slime, it is checked in SlimeEatActionEvent</remarks>
    /// <returns>True - if the slime has started to eat the target, False - if not.</returns>>
    public bool MakeSlimeToEatTarget(EntityUid slime, EntityUid target, double targetStunTime = 20d, SlimeComponent? slimeComp = null)
    {
        if (!Resolve(slime, ref slimeComp, false))
            return false;

        if (!IsDoAfterAllowed(slime, target, slimeComp))
            return false;

        //It is worth disabling the AI during eating so that the DoAfter does not cancel
        if (TryComp<HTNComponent>(slime, out var htn))
        {
            _npc.SleepNPC(slime, htn);
        }

        _stun.TryStun(target, TimeSpan.FromSeconds(targetStunTime), true);

        _transform.SetCoordinates(slime, Transform(target).Coordinates);

        slimeComp.IsEating = true;

        var doAfter = new DoAfterArgs(EntityManager, slime, slimeComp.EatingTime, new SlimeEatingDoAfterEvent(), slime, target, slime)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            Hidden = true,
            RequireCanInteract = true
        };

        return _doAfter.TryStartDoAfter(doAfter);
    }

    /// <summary>
    /// Returns a localized string depending on the stage of the slime's life
    /// </summary>
    /// <remarks>Searches according to the principle: "slime-prefix-name-" + stage.ToLower()</remarks>>
    public string GetLocLifeStage(SlimeLifeStage stage)
    {
        var name = Enum.GetName(stage) ?? "invalid";
        return Loc.GetString(Loc.GetString("slime-prefix-name-" + name.ToLower()));
    }

    /// <summary>
    /// It changes slime's relationship towards a certain target.
    /// <see cref="SlimeComponent.Relationships"/>.
    /// </summary>
    public bool ChangeSlimeRelationship(EntityUid slime, EntityUid target, int amount, SlimeComponent? slimeComp = null)
    {
        if (!Resolve(slime, ref slimeComp, false))
            return false;

        if (slime == target)
            return false;

        if (HasComp<SlimeComponent>(target))
            return false;

        if (slimeComp.Relationships.TryGetValue(target, out var value))
        {
            RaiseLocalEvent(slime, new SlimeChangeRelationshipEvent(slime, target, value + amount, value));
            slimeComp.Relationships[target] = value + amount;
            return true;
        }
        else slimeComp.Relationships.Add(target, 0);

        return false;
    }

    private bool IsDoAfterAllowed(EntityUid slime, EntityUid target, SlimeComponent? slimeComp = null)
    {
        if (!Resolve(slime, ref slimeComp, false))
            return false;

        //Dead can't eat
        if (TryComp<MobStateComponent>(slime, out var mobStateCompSlime) &&
            mobStateCompSlime.CurrentState is not MobState.Alive)
            return false;

        //Corpses don't taste good
        if (TryComp<MobStateComponent>(target, out var mobStateCompTarget) &&
            mobStateCompTarget.CurrentState is MobState.Dead)
            return false;

        //Obesity is bad
        if (TryComp<HungerComponent>(slime, out var hungerComp) &&
            hungerComp.CurrentThreshold is HungerThreshold.Overfed)
            return false;

        //Cannibalism is also bad
        if (HasComp<SlimeComponent>(target))
            return false;

        //You can't eat friends!
        if (slimeComp.Relationships.Any(player => player.Value >= SlimeFriendRelationshipPoints && player.Key == target))
            return false;

        //Suicide is bad
        if (slime == target)
            return false;

        return true;
    }

    private void MakeSlimeToLeaveTarget(EntityUid slime, SlimeComponent? slimeComp = null)
    {
        if (!Resolve(slime, ref slimeComp, false))
            return;

        slimeComp.IsEating = false;

        //If you awaken an NPC when in essence a player, then you get a split personality
        //...and the walking of the entity when the player does not want it
        if (TryComp<HTNComponent>(slime, out var htn) && !HasComp<ActorComponent>(slime))
        {
            _npc.WakeNPC(slime, htn);
        }
    }

    private void SetSlimeBodyAppearanceData(EntityUid slime, SlimeLifeStage stage)
    {
        var stageName = Enum.GetName(stage)
            ?? "Young";

        _appearance.SetData(slime, SlimeVisualState.Body, stageName);
    }

    private void SetSlimeFaceAppearanceData(EntityUid slime, HungerThreshold hungerStage)
    {
        var stageName = Enum.GetName(hungerStage)
            ?? "Young";

        if (_appearance.TryGetData(slime, SlimeVisualState.Face, out var value) && stageName.Equals(value))
            return;

        _appearance.SetData(slime, SlimeVisualState.Face, stageName);
    }

    //Put into a separate method because... why not
    private void CheckCommandOnSlime(SlimeComponent slimeComp)
    {
        if (slimeComp.CommandToCommit == null)
            return;

        if (!_random.Prob(slimeComp.ResponseProbability))
            return;

        var command = slimeComp.CommandToCommit.Value.Item1;
        var args = slimeComp.CommandToCommit.Value.Item2;
        command.Command(args);

        slimeComp.CommandToCommit = null;
    }

    private EntityUid SpawnNearby(EntityCoordinates coords, string prototype, float radius = 1f)
    {
        coords.Position.Deconstruct(out var x, out var y);

        var newX = _random.NextFloat(x - radius, x + radius);
        var funcY = MathF.Sqrt(MathF.Pow(radius, 2f) - MathF.Pow(newX - x, 2f));
        var newY = _random.NextFloat(y - funcY, y + funcY);

        var newCoords = coords.WithPosition(new Vector2(newX, newY));

        var tileRef = newCoords.GetTileRef(EntityManager);
        if (tileRef != null && _turf.IsTileBlocked(tileRef.Value, CollisionGroup.WallLayer))
            newCoords = coords;

        return EntityManager.SpawnEntity(prototype, newCoords);
    }
}
