using Content.Shared.Damage;

namespace Content.Shared._WL.Slippery
{
    [RegisterComponent, AutoGenerateComponentState]
    public sealed partial class HardSlipComponent : Component
    {
        [DataField(required: true), AutoNetworkedField]
        public DamageSpecifier FallDamage = default!;
    }
}
