using System.Numerics;

namespace Content.Server._WL.DayNight
{
    [RegisterComponent]
    public sealed partial class DayNightComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly)]
        [DataField]
        public TimeSpan FullCycle = TimeSpan.FromSeconds(1200);

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("ratio")]
        public Vector2 DayNightRatio = new(6, 4);

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("day")]
        public string DayHex = "#F7CA68FF";

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("night")]
        public string NightHex = "#121330FF";

        [ViewVariables(VVAccess.ReadOnly)]
        public bool WasInit = false;

        [ViewVariables(VVAccess.ReadOnly)]
        public TimeSpan NextCycle;
    }
}
