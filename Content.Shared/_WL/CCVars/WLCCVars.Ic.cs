using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    ///     Количество символов в динамическом описании
    /// </summary>
    public static readonly CVarDef<int> MaxDynamicTextLength =
        CVarDef.Create("ic.dynamic_text_length", 1024, CVar.SERVER | CVar.REPLICATED);
}
