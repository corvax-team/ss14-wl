using Content.Shared.Atmos;

namespace Content.Server._WL.Atmos;

/// <summary>
/// Slowly replenishes a gas tank with a configurable gas mixture.
/// </summary>
[RegisterComponent]
public sealed partial class GasTankRegenerationComponent : Component
{
    [DataField]
    public Dictionary<Gas, float> GasRatios = new()
    {
        [Gas.Oxygen] = 1f,
        [Gas.Nitrogen] = 1f,
    };

    [DataField]
    public float MolesPerSecond = 0.00114f;

    [DataField]
    public float MaxMoles = 2.05137905f;

    [DataField]
    public float MaxPressure = 1000f;
}
