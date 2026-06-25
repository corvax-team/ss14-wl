using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared._WL._Offbrand.Surgery;
using Content.Shared._Offbrand.Surgery;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared._WL.Construction;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;

namespace Content.Server._WL._Offbrand.Surgery;

// this code needs to use predicted popups when construction gets predicted
public sealed partial class ServerSurgeryToolSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedJitteringSystem _jittering = default!;
    [Dependency] private WoundableSystem _woundable = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;

    private const float JitterAmplitude = 10.0f;
    private const float JitterFrequency = 4.0f;
    private const float JitterTime = 2.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryTargetComponent, ToolConstructAttemptedEvent>(OnToolAttemptUse);
        SubscribeLocalEvent<SurgeryTargetComponent, ToolSpeedModifierEvent>(OnToolSpeedModifier);
    }

    private void OnToolAttemptUse(Entity<SurgeryTargetComponent> ent, ref ToolConstructAttemptedEvent args)
    {
        if (!TryComp<SurgeryToolComponent>(args.Used, out var surgTool))
            return;

        var seed = SharedRandomExtensions.HashCodeCombine((int)_timing.CurTick.Value, GetNetEntity(ent).Id);
        var rand = new RobustRandom();

        rand.SetSeed(seed);

        if (!rand.Prob(surgTool.SuccessChance))
        {
            var time = _timing.CurTime;
            if (time <= surgTool.LastFail + surgTool.FailCooldown)
                return;

            args.Cancel();

            if (_prototypeManager.Resolve(surgTool.FailPopups, out var messagePack))
            {
                var message = Loc.GetString(_random.Pick(messagePack.Values),
                        ("toolName", args.Used),
                        ("userName", args.User));
                _popup.PopupEntity(message, ent, PopupType.MediumCaution);
            }

            if (TryComp<WoundableComponent>(ent, out var woundable) && woundable is not null)
            {
                var length = surgTool.FailWounds.Count;

                var shift = rand.Next(0, Math.Max(0, length - 1));

                for (int i = 0; i < length; i++)
                {
                    if (!rand.Prob(surgTool.WoundChance))
                        break;

                    var woundSpecifier = surgTool.FailWounds[(i + shift) % length];

                    _woundable.TryWound((ent, woundable), woundSpecifier.WoundPrototype, woundSpecifier.WoundDamages);
                }
            }

            if (surgTool.FailDamage is not null
                    && TryComp<DamageableComponent>(ent, out var damageable)
                    && damageable is not null)
                _damageable.TryChangeDamage((ent, damageable), surgTool.FailDamage, origin: args.User);

            _jittering.DoJitter(ent, TimeSpan.FromSeconds(JitterTime), false, JitterAmplitude, JitterFrequency);
        }
    }

    private void OnToolSpeedModifier(Entity<SurgeryTargetComponent> ent, ref ToolSpeedModifierEvent args)
    {
        if (!TryComp<SurgeryToolComponent>(args.Tool, out var surgTool))
            return;

        if (surgTool.SpeedModifier is not null)
            args.Speed = args.Speed * surgTool.SpeedModifier.Value;
    }
}
