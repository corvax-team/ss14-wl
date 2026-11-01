using Content.Shared.Input;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Parry;

public abstract partial class SharedParrySystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private SharedPopupSystem _popups = default!;
    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Parry, InputCmdHandler.FromDelegate(enabled: InputParryPressed,
            disabled: InputParryReleased,
            handle: false,
            outsidePrediction: false))
            .Register<SharedParrySystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedParrySystem>();
    }

    private void InputParryPressed(ICommonSession? session)
    {
        Log.Debug("InputParryPressed");

        if (session is null || session.AttachedEntity is not { } user)
            return;

        Log.Debug("InputParryPressed: Session is valid and has attached entity");

        if (!_melee.TryGetWeapon(user, out var weapon, out var _))
            return;

        Log.Debug("InputParryPressed: User has a melee weapon");

        if (!TryStartParry(user, weapon))
            return;

        Log.Debug("InputParryPressed: Parry started");

        _popups.PopupEntity(Loc.GetString("parry-started"), user, user);
    }

    private void InputParryReleased(ICommonSession? session)
    {
        Log.Debug("InputParryReleased");

        if (session is null || session.AttachedEntity is not { } user)
            return;

        Log.Debug("InputParryReleased: Session is valid and has attached entity");

        if (!HasComp<ActiveParryComponent>(user))
            return;

        Log.Debug("InputParryReleased: User has an active parry");

        RemComp<ActiveParryComponent>(user);

        _popups.PopupEntity(Loc.GetString("parry-stopped"), user, user);
    }

    private bool TryStartParry(EntityUid uid, EntityUid weapon)
    {
        if (!HasComp<ActiveParryComponent>(uid))
        {
            var parry = EnsureComp<ActiveParryComponent>(uid);
            parry.Weapon = weapon;
            Dirty(uid, parry);

            return true;
        }

        return false;
    }
}
