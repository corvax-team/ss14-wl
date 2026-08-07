// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._WL.Tajaran;
using Content.Shared.Actions;
using Content.Shared.Audio;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Medical;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server._WL.Tajaran;

public sealed partial class TajaranHairballSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private VomitSystem _vomit = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HairballSpitterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HairballSpitterComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HairballSpitterComponent, HairballActionEvent>(OnHairball);
        SubscribeLocalEvent<HairballSpitterComponent, HairballDoAfterEvent>(OnHairballDoAfter);
        SubscribeLocalEvent<HairballComponent, ThrowDoHitEvent>(OnHairballHit);
        SubscribeLocalEvent<HairballComponent, GettingPickedUpAttemptEvent>(OnHairballPickupAttempt);
    }

    private void OnMapInit(EntityUid uid, HairballSpitterComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.HairballActionEntity, component.HairballActionPrototype);
    }

    private void OnShutdown(EntityUid uid, HairballSpitterComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.HairballActionEntity);
    }

    private void OnHairball(EntityUid uid, HairballSpitterComponent component, HairballActionEvent args)
    {
        if (args.Handled || IsMouthBlocked(uid))
            return;

        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.CoughUpTime,
            new HairballDoAfterEvent(),
            uid);

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupEntity(
            Loc.GetString("tajaran-hairball-cough", ("name", Identity.Entity(uid, EntityManager))),
            uid);
        _audio.PlayPvs(
            "/Audio/_WL/Effects/Species/Tajaran/hairball.ogg",
            uid,
            AudioHelpers.WithVariation(0.15f));

        args.Handled = true;
    }

    private void OnHairballDoAfter(EntityUid uid, HairballSpitterComponent component, HairballDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || IsMouthBlocked(uid))
            return;

        SpawnHairball(uid, component);
        args.Handled = true;
    }

    private bool IsMouthBlocked(EntityUid uid)
    {
        if (!_inventory.TryGetSlotEntity(uid, "mask", out var mask) ||
            !TryComp<IngestionBlockerComponent>(mask, out var blocker) ||
            !blocker.Enabled)
            return false;

        _popup.PopupEntity(
            Loc.GetString("tajaran-hairball-mask", ("mask", mask.Value)),
            uid,
            uid,
            PopupType.SmallCaution);
        return true;
    }

    private void SpawnHairball(EntityUid uid, HairballSpitterComponent component)
    {
        var hairball = Spawn(component.HairballPrototype, Transform(uid).Coordinates);

        if (!TryComp<HairballComponent>(hairball, out var hairballComponent) ||
            !TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        var chemicals = _bloodstream.FlushChemicals((uid, bloodstream), 20);
        if (chemicals == null ||
            !_solutions.TryGetSolution(hairball, hairballComponent.SolutionName, out var solution))
            return;

        _solutions.TryAddSolution(solution.Value, chemicals);
    }

    private void OnHairballHit(EntityUid uid, HairballComponent component, ThrowDoHitEvent args)
    {
        if (HasComp<HairballSpitterComponent>(args.Target) ||
            !HasComp<StatusEffectsComponent>(args.Target) ||
            !_random.Prob(0.2f))
            return;

        _vomit.Vomit(args.Target);
    }

    private void OnHairballPickupAttempt(
        EntityUid uid,
        HairballComponent component,
        GettingPickedUpAttemptEvent args)
    {
        if (HasComp<HairballSpitterComponent>(args.User) ||
            !HasComp<StatusEffectsComponent>(args.User) ||
            !_random.Prob(0.2f))
            return;

        _vomit.Vomit(args.User);
        args.Cancel();
    }
}
