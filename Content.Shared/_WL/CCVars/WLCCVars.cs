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
     * Chat Gpt
     */
    /// <summary>
    /// Ссылка, на которую будут отправляться запросы от клиента OpenAi.
    /// </summary>
    public static readonly CVarDef<string> GptQueriesEndpoint =
        CVarDef.Create("gpt.endpoint", "https://api.proxyapi.ru/openai/v1/chat/completions", CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.SERVER);

    /// <summary>
    /// Работает(включен) ли ChatGptManager на данный момент.
    /// </summary>
    public static readonly CVarDef<bool> IsGptEnabled =
        CVarDef.Create("gpt.enabled", true, CVar.REPLICATED);

    /// <summary>
    /// Чат-модель, которая будет использоваться для отправки запросов.
    /// </summary>
    public static readonly CVarDef<string> GptChatModel =
        CVarDef.Create("gpt.chat_model", "gpt-4o-mini", CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.SERVER);

    /// <summary>
    /// Максимальное количество токенов, которое может вернуть в ответе на запрос ИИ.
    /// </summary>
    public static readonly CVarDef<int> GptMaxTokens =
        CVarDef.Create("gpt.max_tokens", 250, CVar.SERVERONLY | CVar.SERVER);

    /// <summary>
    /// Путь, по которому можно получить баланс аккаунта.
    /// </summary>
    public static readonly CVarDef<string> GptBalanceMap =
        CVarDef.Create("gpt.balance_map", "https://api.proxyapi.ru/proxyapi/balance", CVar.SERVERONLY | CVar.SERVER);

    /*
     * Central Command AI
     */
    /// <summary>
    /// Максимальное количество запросов на ЦК в минуту, на которые будет дан ответ.
    /// </summary>
    public static readonly CVarDef<int> CCMaxQueriesPerMinute =
        CVarDef.Create("central_command.max_queries_per_minute", 6, CVar.SERVERONLY | CVar.SERVER);

    /// <summary>
    /// Максимальное время ответа на факс.
    /// В секундах.
    /// </summary>
    public static readonly CVarDef<int> CCMaxResponseTime =
        CVarDef.Create("central_command.max_response_time", 650, CVar.SERVERONLY); //It has normal distribution random where medium is 450 second (7.5 min)

    /// <summary>
    /// Минимальное время ответа на факс.
    /// В секундах.
    /// </summary>
    public static readonly CVarDef<int> CCMinResponseTime =
        CVarDef.Create("central_command.min_response_time", 250, CVar.SERVERONLY); //It has normal distribution random where medium is 450 second (7.5 min)

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
}
