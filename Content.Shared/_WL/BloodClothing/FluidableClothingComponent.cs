using Robust.Shared.GameStates;

namespace Content.Shared._WL.BloodClothing
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class FluidableClothingComponent : Component
    {
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string Solution = "CLOTHING_FLUID_STORAGE";

        [DataField, ViewVariables(VVAccess.ReadOnly)]
        public float MaxFluidTakeAtPercentage = 0.45f;
    }
}
