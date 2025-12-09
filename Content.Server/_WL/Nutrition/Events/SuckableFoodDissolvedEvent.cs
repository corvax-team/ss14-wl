using Content.Server._WL.Nutrition.Components;
using Robust.Shared.Containers;

namespace Content.Server._WL.Nutrition.Events;

public sealed partial class SuckableFoodDissolvedEvent : EntityEventArgs
{
    public Entity<SuckableFoodComponent> Suckable { get; }
    public BaseContainer Container { get; }

    public EntityUid Sucker { get; }

    public SuckableFoodDissolvedEvent(Entity<SuckableFoodComponent> suckable, BaseContainer container, EntityUid sucker)
    {
        Suckable = suckable;
        Container = container;
        Sucker = sucker;
    }
}
