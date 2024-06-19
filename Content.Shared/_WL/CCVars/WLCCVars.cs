using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
[CVarDefs]
public sealed class WLCVars
{
    /*
     * Госты
     */
    /// <summary>
    /// Через сколько времени(в секундах) появится кнопка возвращения в лобби.
    /// </summary>
    public static readonly CVarDef<int> GhostReturnToLobbyButtonCooldown =
        CVarDef.Create("ghost.return_to_lobby_button_cooldown", 1200, CVar.SERVERONLY);

    /*
     * Управление
     */
    /// <summary>
    /// Бегает ли игрок на кнопку shift или идёт.
    /// </summary>
    public static readonly CVarDef<bool> RunningOnShift =
        CVarDef.Create("move.running_on_shift", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
