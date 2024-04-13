using Content.Shared._WL.Slimes.Components;

namespace Content.Server.Medical.BiomassReclaimer;

public enum BiomassReclaimerProcessType : byte
{
    /// <summary>
    /// Processes corpses into biomass.
    /// </summary>
    Biomass,

    /// <summary>
    /// Recycles corpses into rehydratable cubes, if any.
    /// </summary>
    Cube,

    /// <summary>
    /// Processes the corpses of slimes into cores.
    /// </summary>
    Core
}
