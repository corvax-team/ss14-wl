using Content.Server._WL.Economics;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Economics.Prototypes
{
    [Prototype("salaryConfiguration")]
    public sealed partial class SalaryGettingConfigurationPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(required: true)]
        public List<EconomicWageCoefficient> Coefficients = new();
    }
}
