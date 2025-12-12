using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    /// ГАНС! ЕСЛИ ОНИ ВДРУГ НЕ НУЖНЫ ТО ПЕРЕКЛЮЧИ ПЕРЕКЛЮЧАТЕЛЬ!!!
    /// </summary>
    public static readonly CVarDef<bool> SkillsEnabled =
        CVarDef.Create("skills.enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
}
