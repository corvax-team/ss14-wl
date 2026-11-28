using Content.Shared._WL.Combat;
using Robust.Shared.Map;

namespace Content.Client._WL.Combat;

public sealed partial class AdvancedCombatSystem : SharedAdvancedCombatSystem
{
    protected override bool ThrowHeldItem(EntityUid uid, EntityCoordinates coordinates) { return false; }
}
