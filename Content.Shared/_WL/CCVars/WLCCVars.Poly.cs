using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    /// Интервал, через который Поли™ будет готова выбрать новое сообщение!
    /// </summary>
    public static readonly CVarDef<int> PolyMessageChooseCooldown =
        CVarDef.Create("poly.choose_cooldown_time", 3600, CVar.SERVERONLY,
            "Интервал, через который Поли™ будет готова выбрать новое сообщение!");

    /// <summary>
    /// Нужна ли очистка выбранных Поли™ сообщений после РАУНДА.
    /// </summary>
    public static readonly CVarDef<bool> PolyNeededRoundEndCleanup =
        CVarDef.Create("poly.round_end_cleanup", false, CVar.SERVERONLY,
            "Нужна ли очистка выбранных Поли™ сообщений после РАУНДА.");
}
