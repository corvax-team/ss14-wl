using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
[CVarDefs]
public sealed class WLCVars
{
    /*
     *  Game
     */
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

    /*
     *  HTTP API
     */
    /// <summary>
    /// Токен для авторизации htpp-запросов на api сервера.
    /// </summary>
    public static readonly CVarDef<string> WLApiToken =
        CVarDef.Create(
            "admin.wl_api_token", string.Empty,
            CVar.SERVERONLY | CVar.CONFIDENTIAL,
            "Строковой токен, использующийся для авторизации HTTP-запросов, отправленных на http API сервера.");

    /*
     *  Discord
     */
    /// <summary>
    /// Через какое время все токены на подключение аккаунта игры к дискорду будут недействительны.
    /// </summary>
    public static readonly CVarDef<int> DiscordAuthTokensExpirationTime =
        CVarDef.Create("discord.auth_tokens_expiration_time", 300, CVar.REPLICATED);

    /*
     * Poly
     */
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

    /*
      * Vote
      */
    /// <summary>
    /// Доступна ли игрокам возможность вызвать шаттл голосованием?
    /// </summary>
    public static readonly CVarDef<bool> VoteShuttleEnabled =
        CVarDef.Create("vote.evacuation_shuttle_vote_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// Сколько требуется согласных игроков для вызова.
    /// В процентах.
    /// </summary>
    public static readonly CVarDef<float> VoteShuttlePlayersRatio =
        CVarDef.Create("vote.evacuation_shuttle_vote_ratio", 0.6f, CVar.SERVERONLY);

    /// <summary>
    /// Время голосования.
    /// </summary>
    public static readonly CVarDef<int> VoteShuttleTimer =
        CVarDef.Create("vote.evacuation_shuttle_vote_time", 40, CVar.SERVERONLY);

    /*
     * Ic
     */

    public static readonly CVarDef<int> MaxDynamicTextLength =
        CVarDef.Create("ic.dynamic_text_length", 1024, CVar.SERVER | CVar.REPLICATED);

    /*
     * Skills
     */
    /// <summary>
    /// ГАНС! ЕСЛИ ОНИ ВДРУГ НЕ НУЖНЫ ТО ПЕРЕКЛЮЧИ ПЕРЕКЛЮЧАТЕЛЬ!!!
    /// </summary>
    public static readonly CVarDef<bool> SkillsEnabled =
        CVarDef.Create("skills.enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
}
