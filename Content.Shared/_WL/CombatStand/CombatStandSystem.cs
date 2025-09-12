using System.Linq;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Standing;
using Robust.Shared.Collections;
using Robust.Shared.Timing;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.CombatStand;

public sealed class CombatStandSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunStandingRequiredComponent, ShotAttemptedEvent>(OnShootAttempt);
        SubscribeLocalEvent<GunStandingRequiredComponent, ExaminedEvent>(OnExamineRequires);
    }

    private void OnShootAttempt(EntityUid uid, GunStandingRequiredComponent component, ref ShotAttemptedEvent args)
    {
        if (TryComp<StandingStateComponent>(args.User, out var standable) &&
            !standable.Standing &&
            TryComp<GunStandingRequiredComponent>(uid, out var standreq)
            )
        {
            args.Cancel();

            var time = _timing.CurTime;
            if (time > component.LastPopup + component.PopupCooldown)
            {
                component.LastPopup = time;
                var message = Loc.GetString("standing-component-requires", ("item", uid));

                _popup.PopupClient(message, args.Used, args.User);
            }
        }
    }

    private void OnExamineRequires(Entity<GunStandingRequiredComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.StandRequiresExamineMessage != null)
            args.PushText(Loc.GetString(entity.Comp.StandRequiresExamineMessage));
    }

}


[RegisterComponent, NetworkedComponent, Access(typeof(CombatStandSystem)), AutoGenerateComponentState]
public sealed partial class GunStandingRequiredComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan LastPopup;

    [DataField, AutoNetworkedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    [DataField]
    public LocId? StandRequiresExamineMessage  = "gunstandingrequired-component-examine";
}
