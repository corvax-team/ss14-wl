using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    ///     Max number of strokes in a single paper drawing. Extra strokes are dropped on save.
    ///     Also limits drawing in the client's PaperDrawingControl.
    /// </summary>
    public static readonly CVarDef<int> PaperMaxStrokes =
        CVarDef.Create("paper.max_drawing_strokes", 512, CVar.SERVER | CVar.REPLICATED,
            "Max number of strokes in a single paper drawing.");

    /// <summary>
    ///     Max total number of points in a single paper drawing. Extra points are dropped on save.
    ///     Also limits drawing in the client's PaperDrawingControl.
    /// </summary>
    public static readonly CVarDef<int> PaperMaxPoints =
        CVarDef.Create("paper.max_drawing_points", 8192, CVar.SERVER | CVar.REPLICATED,
            "Max total number of points in a single paper drawing.");
}
