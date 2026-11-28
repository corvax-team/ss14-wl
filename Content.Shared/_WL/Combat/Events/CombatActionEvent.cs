using Robust.Shared.Serialization;

namespace Content.Shared._WL.Combat;

public sealed class CombatActionEvent : EntityEventArgs
{
    public EntityUid Weapon { get; }

    public EntityUid? Target { get; }

    public CombatIntent Intent { get; }

    public bool Handled { get; private set; }

    public CombatActionEvent(EntityUid weapon, EntityUid? target, CombatIntent intent)
    {
        Weapon = weapon;
        Target = target;
        Intent = intent;
    }

    public void Handle()
    {
        Handled = true;
    }
}
