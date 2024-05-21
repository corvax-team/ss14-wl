using Content.Shared._WL.Economics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    public sealed partial class GettingSalaryComponent : Component
    {
        [DataField(required: true)]
        public float Salary;

        [DataField]
        public float Interval = 1f;

        [DataField]
        public bool UseCoefficients = true;

        [ViewVariables]
        [AutoNetworkedField]
        public List<EconomicPenalty> Penalties = new();

        [ViewVariables]
        public TimeSpan NextGetTime = TimeSpan.Zero;
    }
}
