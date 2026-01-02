using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
public sealed partial class WLCVars
{
    /// <summary>
    /// Через сколько времени(в секундах) появится кнопка возвращения в лобби.
    /// </summary>
    public static readonly CVarDef<int> GhostReturnToLobbyButtonCooldown =
        CVarDef.Create("ghost.return_to_lobby_button_cooldown", 600, CVar.SERVERONLY); //WL-changes  (10 minutes)

    /// <summary>
    /// Нужно ли проверять игрока на возраст при выборе роли.
    /// </summary>
    public static readonly CVarDef<bool> IsAgeCheckNeeded =
        CVarDef.Create("game.is_age_check_needed", true, CVar.REPLICATED);

    /// <summary>
    /// Включены ли проверки ограничений на выбор роли.
    /// </summary>
    public static readonly CVarDef<bool> RoleRestrictionChecksEnabled =
        CVarDef.Create("game.role_restriction_checks_enabled", true, CVar.REPLICATED | CVar.SERVER);
}
