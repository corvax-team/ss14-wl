namespace Content.Shared._WL.Slippery
{
    [RegisterComponent]
    public sealed partial class HardSlipComponent : Component
    {
        [DataField("fallDamage")]
        [Access(Other = AccessPermissions.ReadWrite)]
        public float FallDamage = 1f;
    }
}
