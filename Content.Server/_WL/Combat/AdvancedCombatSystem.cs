using Content.Server.Hands.Systems;
using Content.Shared._WL.Combat;
using Robust.Shared.Map;

namespace Content.Server._WL.Combat;

public sealed partial class AdvancedCombatSystem : SharedAdvancedCombatSystem
{
    [Dependency] private HandsSystem _hands = default!;

    protected override bool ThrowHeldItem(EntityUid uid, EntityCoordinates coordinates) =>
        _hands.ThrowHeldItem(uid, coordinates);
}
