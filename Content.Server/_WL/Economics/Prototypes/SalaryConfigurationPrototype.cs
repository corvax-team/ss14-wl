using Content.Server._WL.Economics.Systems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Economics.Prototypes
{
    [Prototype("salaryConfiguration")] // Если вы захотите поменять название, то также поменяйте его и в EntryPoint.cs, чтобы клиентская сторона игнорировала его.
    [Access([typeof(EconomicSystem)])]
    public sealed class SalaryConfigurationPrototype : IPrototype
    {
        [IdDataField] public string ID { get; private init; } = default!;

        [DataField(required: true, serverOnly: true)]
        public List<EconomicWageCoefficient> Coefficients = new();
    }
}
