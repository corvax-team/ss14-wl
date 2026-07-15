using Content.Shared._WL.SafeCode;
using Content.Shared.Damage.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server._WL.SafeCode;

public sealed partial class SafeCodeSystem : SharedSafeCodeSystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SafeCodeComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<SafeCodeComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<SafeCodeComponent, SafeCodeRequestMessage>(OnCodeRequest);
        SubscribeLocalEvent<SafeCodeComponent, SafeCodeLockRequestMessage>(OnLockRequest);

        SubscribeLocalEvent<SafeCodeComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnInit(Entity<SafeCodeComponent> ent, ref MapInitEvent args)
    {
        var maxValue = (int) Math.Pow(10, ent.Comp.CodeLength);
        var code = _random.Next(maxValue);

        ent.Comp.Code = code.ToString().PadLeft(ent.Comp.CodeLength, '0');

        _appearance.SetData(ent.Owner, SafeCodeVisuals.Locked, ent.Comp.Locked);
        _appearance.SetData(ent.Owner, SafeCodeVisuals.Broken, ent.Comp.Broken);
    }

    private void OnUiOpened(Entity<SafeCodeComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnCodeRequest(Entity<SafeCodeComponent> ent, ref SafeCodeRequestMessage args)
    {
        var correct = args.Code == ent.Comp.Code;
        if (correct)
        {
            SetLocked(ent, false);
            UpdateUi(ent, correct);

            _uiSystem.CloseUi(ent.Owner, SafeCodeUiKey.Key, args.Actor);
            return;
        }

        _audio.PlayPvs(ent.Comp.FailSound, ent.Owner);
        UpdateUi(ent, correct);
    }

    private void OnLockRequest(Entity<SafeCodeComponent> ent, ref SafeCodeLockRequestMessage args)
    {
        ent.Comp.Locked = true;
        SetLocked(ent, true);
        UpdateUi(ent);

        _uiSystem.CloseUi(ent.Owner, SafeCodeUiKey.Key, args.Actor);
    }

    private void OnEmagged(Entity<SafeCodeComponent> ent, ref GotEmaggedEvent args)
    {
        if (ent.Comp.Broken)
            return;

        SetBroken(ent, true);
        SetLocked(ent, false);

        _uiSystem.CloseUi(ent.Owner, SafeCodeUiKey.Key);
        RemComp<ActivatableUIComponent>(ent.Owner);

        args.Handled = true;
    }

    private void UpdateUi(Entity<SafeCodeComponent> ent, bool? lastAttempt = null)
    {
        var state = new SafeCodeBoundUserInterfaceState(
            ent.Comp.CodeLength,
            ent.Comp.Locked,
            lastAttempt
        );

        _uiSystem.SetUiState(ent.Owner, SafeCodeUiKey.Key, state);
    }

    /// <summary>
    /// Sets the safe broken state and updates its visual appearance.
    /// </summary>
    public void SetBroken(Entity<SafeCodeComponent> ent, bool broken)
    {
        ent.Comp.Broken = broken;
        Dirty(ent);

        _appearance.SetData(ent.Owner, SafeCodeVisuals.Broken, broken);
    }

    /// <summary>
    /// Sets the safe lock state and updates its visual appearance.
    /// </summary>
    public void SetLocked(Entity<SafeCodeComponent> ent, bool locked)
    {
        ent.Comp.Locked = locked;
        Dirty(ent);

        _appearance.SetData(ent.Owner, SafeCodeVisuals.Locked, locked);
    }
}
