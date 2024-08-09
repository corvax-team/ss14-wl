using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.BloodClothing
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class FluidableClothingComponent : Component
    {
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string Solution = "CLOTHING_FLUID_STORAGE";

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float MaxFluidTakeAtPercentage = 0.45f;

        [Access(typeof(SharedFluidOnClothingSystem))]
        [ViewVariables(VVAccess.ReadOnly)]
        public Entity<SolutionComponent>? AbsorbingEntity = null;
    }
}
