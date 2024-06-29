using Content.Server.Power.Components;

namespace Content.Server._WL.PulseDemon.Components;

[RegisterComponent]
public sealed partial class MarkeredCableComponent : Component
{
    public Entity<CableComponent>? Cable = null;
}
