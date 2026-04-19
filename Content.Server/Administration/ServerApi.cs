using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Content.Server._WL.DiscordAuth;
using Content.Server._WL.Poly;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Presets;
using Content.Server.Maps;
using Content.Server.RoundEnd;
using Content.Shared._WL.CCVars;
using Content.Shared.Administration;
using Content.Shared.Administration.Managers;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Prototypes;
using Robust.Server;
using Robust.Server.ServerStatus;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Administration;

/// <summary>
/// Exposes various admin-related APIs via the game server's <see cref="StatusHost"/>.
/// </summary>
public sealed partial class ServerApi : IPostInjectInit
{
    private const string SS14TokenScheme = "SS14Token";

    //WL-Changes-start
    private const string WLAuthTokenScheme = "WLCorvaxToken"; //ихихихих
    //WL-Changes-end

    private static readonly HashSet<string> PanicBunkerCVars =
    [
        CCVars.PanicBunkerEnabled.Name,
        CCVars.PanicBunkerDisableWithAdmins.Name,
        CCVars.PanicBunkerEnableWithoutAdmins.Name,
        CCVars.PanicBunkerCountDeadminnedAdmins.Name,
        CCVars.PanicBunkerShowReason.Name,
        CCVars.PanicBunkerMinAccountAge.Name,
        CCVars.PanicBunkerMinOverallMinutes.Name,
        CCVars.PanicBunkerCustomReason.Name,
    ];

    [Dependency] private readonly IStatusHost _statusHost = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly ISharedAdminManager _adminManager = default!;
    [Dependency] private readonly IGameMapManager _gameMapManager = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IBanManager _bans = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    //WL-Changes-start
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IBaseServer _baseServer = default!;
    //WL-Changes-end

    //WL-Changes-start
    private static readonly ulong[] StuffBotIds = [1227044346566541322];
    //WL-Changes-end

    private string _corvax_token = string.Empty;

    //WL-Changes-start
    private string _wl_token = string.Empty;
    //WL-Changes-end

    private ISawmill _sawmill = default!;

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("serverApi");

        // Get
        RegisterActorHandler(HttpMethod.Get, "/admin/info", InfoHandler);
        RegisterHandler(HttpMethod.Get, "/admin/game_rules", GetGameRules);
        RegisterHandler(HttpMethod.Get, "/admin/presets", GetPresets);

        // Post
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/round/start", ActionRoundStart);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/round/end", ActionRoundEnd);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/round/restartnow", ActionRoundRestartNow);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/kick", ActionKick);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/ban", ActionBan);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/add_game_rule", ActionAddGameRule);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/end_game_rule", ActionEndGameRule);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/force_preset", ActionForcePreset);
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/set_motd", ActionForceMotd);
        RegisterActorHandler(HttpMethod.Patch, "/admin/actions/panic_bunker", ActionPanicPunker);

        //WL-Changes-start
        RegisterActorHandler(HttpMethod.Post, "/admin/actions/ahelp", ActionAhelp);
        RegisterHandler(HttpMethod.Post, "/player/actions/link/account", LinkDiscordAccount);

        RegisterHandler(HttpMethod.Post, "/player/info/discord", GetLinkedAccount);

        RegisterActorHandler(HttpMethod.Patch, "/admin/actions/server/shutdown", ShutdownServer);

        RegisterHandler(HttpMethod.Get, "/admin/info/poly/random_message", PolyMessage);

        RegisterParameterizedHandler(HttpMethod.Get, $"/admin/info/poly/images/{{${Constants.PolyMapImage}}}.png", PolyImage);
        //wL-Changes-end
    }

    public void Initialize()
    {
        _config.OnValueChanged(CCVars.AdminApiToken, UpdateCorvaxToken, true);

        //WL-Changes-start
        _config.OnValueChanged(WLCVars.WLApiToken, UpdateWLToken, true);
        //WL-Changes-end
    }

    public void Shutdown()
    {
        _config.UnsubValueChanged(CCVars.AdminApiToken, UpdateCorvaxToken);

        //WL-Changes-start
        _config.UnsubValueChanged(WLCVars.WLApiToken, UpdateWLToken);
        //WL-Changes-end
    }

    private void UpdateCorvaxToken(string token)
    {
        _corvax_token = token;
    }

    //WL-Changes-start
    private void UpdateWLToken(string token)
    {
        _wl_token = token;
    }
    //WL-Changes-end

    #region Actions

    //WL-Changes-start
    private async Task PolyMessage(IStatusHandlerContext context)
    {
        var poly_system = _entitySystemManager.GetEntitySystem<PolySystem>();

        var entry = poly_system.Pick();

        if (entry == null)
        {
            var is_ready = poly_system.IsReadyToPick();
            var how_long = poly_system.HowLongBeforeReady();

            var msg = is_ready
                ? "Поли ожидает подходящего сообщения!"
                : $"Поли устала! До готовности: {how_long}";

            await RespondError(
                context,
                ErrorCode.ServiceUnavailable,
                HttpStatusCode.ServiceUnavailable,
                msg);

            return;
        }

        await context.RespondJsonAsync(entry.Value);
    }

    private async Task PolyImage(IStatusHandlerContext context, Dictionary<string, string> maps)
    {
        var poly_system = _entitySystemManager.GetEntitySystem<PolySystem>();

        if (!maps.TryGetValue(Constants.PolyMapImage, out var map))
        {
            await RespondError(
                context,
                ErrorCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError,
                "Ошибка при получении ссылки!");
            return;
        }

        using var stream = poly_system.PickImage(map);

        if (stream == null)
        {
            await RespondError(
                context,
                ErrorCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError,
                "Изображение не было найдено!");
            return;
        }

        await using var resp_stream = await context.RespondStreamAsync();

        stream.CopyTo(resp_stream);
    }

    private async Task ShutdownServer(IStatusHandlerContext context, Actor actor)
    {
        if (!await IsAdmin(actor.Record.UserId))
        {
            await RespondBadRequest(context, "Вы не являетесь администратором!");
            return;
        }

        await RunOnMainThread(async () =>
        {
            _adminLog.Add(LogType.WLHttpApi, LogImpact.Extreme, $"Администратор {actor.Record.LastSeenUserName} перезапустил сервер с помощью HTTP api.");

            _baseServer.Shutdown("Сервер был перезапущен администратором!");
        });
    }

    private async Task GetLinkedAccount(IStatusHandlerContext context)
    {
        var http_body = await ReadJson<InnerActor>(context);
        if (http_body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var linked = await _db.GetPlayerByDiscordId(http_body.DiscordId, default);
            if (linked == null)
            {
                await RespondError(context, ErrorCode.PlayerNotFound, HttpStatusCode.BadRequest, "Текущий аккаунт не привязан к игровому аккаунту!");
                return;
            }

            var body = new
            {
                Guid = linked.UserId.UserId.ToString(),
                Username = linked.LastSeenUserName
            };

            await context.RespondJsonAsync(body, HttpStatusCode.OK);
        });
    }

    private async Task LinkDiscordAccount(IStatusHandlerContext context)
    {
        var body = await ReadJson<LinkUserDiscordBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var auth = _entitySystemManager.GetEntitySystem<DiscordAuthSystem>();

            var username = body.Login;
            var discord_user_id = body.User;
            var code = body.Code;

            if (!_playerManager.TryGetSessionByUsername(username, out var session))
            {
                await RespondBadRequest(context, "Указанного игрока нет на сервере!");
                return;
            }

            //if (await _db.IsLinkedToDiscord(session.UserId, default))
            //{
            //    await RespondBadRequest(context, "Текущий игровой аккаунт уже привязан к дискорд-аккаунту!");
            //    return;
            //}

            if (await _db.GetPlayerDiscordId(session.UserId, default) != null)
            {
                await RespondBadRequest(context, "Текущий игровой аккаунт уже привязан к дискорд-аккаунту!");
                return;
            }

            var check_code = auth.GetUserCode(session.UserId);
            if (check_code == null)
            {
                await RespondError(context, ErrorCode.PlayerNotFound, HttpStatusCode.InternalServerError, "Уникальный код указанного игрока равен <NULL>");
                return;
            }

            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(check_code), Encoding.UTF8.GetBytes(code)))
            {
                await RespondBadRequest(context, "Указанный уникальный код недействителен!");
                return;
            }

            await _db.LinkPlayerDiscord(session.UserId, discord_user_id, default);

            _sawmill.Info($"Игрок {session.Name} подключил к игровому аккаунту дискорд-аккаунт с ID {discord_user_id}.");

            await RespondOk(context);
        });
    }

    private async Task ActionAhelp(IStatusHandlerContext context, Actor actor)
    {
        var body = await ReadJson<AhelpBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var bwoink = _entitySystemManager.GetEntitySystem<BwoinkSystem>();
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();

            var targetUsername = body.TargetUsername;
            var senderNetId = actor.Record;

            if (!_playerManager.TryGetSessionByUsername(targetUsername, out var session))
            {
                await RespondBadRequest(context, "Указанного guid игрока нет на сервере на данный момент.");
                return;
            }

            var record = actor.Record;

            var sent = await bwoink.HandleDiscordAhelp(new(session.UserId, senderNetId.UserId, body.Message),
                record.LastSeenUserName,
                record.UserId,
                !actor.IsStuffBot
            );

            _sawmill.Info($"Администратор {record.LastSeenUserName} дистанционно отправил сообщение \"{body.Message}\" игроку {session.Name}");

            await (sent ? RespondOk(context) : RespondError(context, ErrorCode.BadRequest, HttpStatusCode.NotAcceptable, "Вы не являетесь администратором!"));
        });
    }
    //WL-Changes-end

    /// <summary>
    ///     Changes the panic bunker settings.
    /// </summary>
    private async Task ActionPanicPunker(IStatusHandlerContext context, Actor actor)
    {
        var request = await ReadJson<JsonObject>(context);
        if (request == null)
            return;

        var toSet = new Dictionary<string, object>();
        foreach (var (cVar, value) in request)
        {
            if (!PanicBunkerCVars.Contains(cVar))
            {
                await RespondBadRequest(context, $"Invalid panic bunker CVar: '{cVar}'");
                return;
            }

            if (value == null)
            {
                await RespondBadRequest(context, $"Value is null: '{cVar}'");
                return;
            }

            if (value is not JsonValue jsonValue)
            {
                await RespondBadRequest(context, $"Value is not valid: '{cVar}'");
                return;
            }

            object castValue;
            var cVarType = _config.GetCVarType(cVar);
            if (cVarType == typeof(bool))
            {
                if (!jsonValue.TryGetValue(out bool b))
                {
                    await RespondBadRequest(context, $"CVar '{cVar}' must be of type bool.");
                    return;
                }

                castValue = b;
            }
            else if (cVarType == typeof(int))
            {
                if (!jsonValue.TryGetValue(out int i))
                {
                    await RespondBadRequest(context, $"CVar '{cVar}' must be of type int.");
                    return;
                }

                castValue = i;
            }
            else if (cVarType == typeof(string))
            {
                if (!jsonValue.TryGetValue(out string? s))
                {
                    await RespondBadRequest(context, $"CVar '{cVar}' must be of type string.");
                    return;
                }

                castValue = s;
            }
            else
            {
                throw new NotSupportedException("Unsupported CVar type");
            }

            toSet[cVar] = castValue;
        }

        await RunOnMainThread(() =>
        {
            foreach (var (cVar, value) in toSet)
            {
                _config.SetCVar(cVar, value);
                _sawmill.Info(
                    $"Panic bunker property '{cVar}' changed to '{value}' by {FormatLogActor(actor)}.");
            }
        });

        await RespondOk(context);
    }

    /// <summary>
    ///     Sets the current MOTD.
    /// </summary>
    private async Task ActionForceMotd(IStatusHandlerContext context, Actor actor)
    {
        var motd = await ReadJson<MotdActionBody>(context);
        if (motd == null)
            return;

        _sawmill.Info($"MOTD changed to \"{motd.Motd}\" by {FormatLogActor(actor)}.");

        await RunOnMainThread(() => _config.SetCVar(CCVars.MOTD, motd.Motd));
        // A hook in the MOTD system sends the changes to each client
        await RespondOk(context);
    }

    /// <summary>
    ///     Forces the next preset-
    /// </summary>
    private async Task ActionForcePreset(IStatusHandlerContext context, Actor actor)
    {
        var body = await ReadJson<PresetActionBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();
            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                await RespondError(
                    context,
                    ErrorCode.InvalidRoundState,
                    HttpStatusCode.Conflict,
                    "Game must be in pre-round lobby");
                return;
            }

            var preset = ticker.FindGamePreset(body.PresetId);
            if (preset == null)
            {
                await RespondError(
                    context,
                    ErrorCode.GameRuleNotFound,
                    HttpStatusCode.UnprocessableContent,
                    $"Game rule '{body.PresetId}' doesn't exist");
                return;
            }

            ticker.SetGamePreset(preset);
            _sawmill.Info($"Forced the game to start with preset {body.PresetId} by {FormatLogActor(actor)}.");

            await RespondOk(context);
        });
    }

    /// <summary>
    ///     Ends an active game rule.
    /// </summary>
    private async Task ActionEndGameRule(IStatusHandlerContext context, Actor actor)
    {
        var body = await ReadJson<GameRuleActionBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();
            var gameRule = ticker
                .GetActiveGameRules()
                .FirstOrNull(rule =>
                    _entityManager.MetaQuery.GetComponent(rule).EntityPrototype?.ID == body.GameRuleId);

            if (gameRule == null)
            {
                await RespondError(context,
                    ErrorCode.GameRuleNotFound,
                    HttpStatusCode.UnprocessableContent,
                    $"Game rule '{body.GameRuleId}' not found or not active");

                return;
            }

            _sawmill.Info($"Ended game rule {body.GameRuleId} by {FormatLogActor(actor)}.");
            ticker.EndGameRule(gameRule.Value);

            await RespondOk(context);
        });
    }

    /// <summary>
    ///     Adds a game rule to the current round.
    /// </summary>
    private async Task ActionAddGameRule(IStatusHandlerContext context, Actor actor)
    {
        var body = await ReadJson<GameRuleActionBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();
            if (!_prototypeManager.HasIndex<EntityPrototype>(body.GameRuleId))
            {
                await RespondError(context,
                    ErrorCode.GameRuleNotFound,
                    HttpStatusCode.UnprocessableContent,
                    $"Game rule '{body.GameRuleId}' not found or not active");
                return;
            }

            var ruleEntity = ticker.AddGameRule(body.GameRuleId);
            _sawmill.Info($"Added game rule {body.GameRuleId} by {FormatLogActor(actor)}.");
            if (ticker.RunLevel == GameRunLevel.InRound)
            {
                ticker.StartGameRule(ruleEntity);
                _sawmill.Info($"Started game rule {body.GameRuleId} by {FormatLogActor(actor)}.");
            }

            await RespondOk(context);
        });
    }

    /// <summary>
    ///     Bans a player.
    /// </summary>
    private async Task ActionBan(IStatusHandlerContext context, Actor actor)
    {
        var body = await ReadJson<BanActionBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            var located = await _locator.LookupIdByNameOrIdAsync(body.Guid.ToString());

            if (located == null)
            {
                await RespondError(
                    context,
                    ErrorCode.PlayerNotFound,
                    HttpStatusCode.UnprocessableContent,
                    "Player not found");
                return;
            }

            var bans = await _db.GetBansAsync(userId: located.UserId,
                address: null,
                hwId: null,
                modernHWIds: located.LastModernHWIds,
                includeUnbanned: false);
            if (bans.Count > 0)
            {
                await RespondError(
                    context,
                    ErrorCode.PlayerAlreadyBanned,
                    HttpStatusCode.Conflict,
                    "Player is already banned.");
                return;
            }

            var reason = body.Reason ?? "No reason supplied";
            var info = new CreateServerBanInfo(reason);

            info.AddHWId(located.LastHWId);
            info.AddUser(located.UserId, located.Username);
            info.WithSeverity(body.Severity);
            if (body.Minutes != null && body.Minutes != 0)
            {
                info.WithMinutes(body.Minutes.Value);
            }

            info.WithBanningAdmin(new NetUserId(actor.Guid));

            // Add the ip if the user is currently connected.
            if (_playerManager.TryGetSessionById(new NetUserId(body.Guid), out var player))
            {
                info.AddAddress(player.Channel.RemoteEndPoint.Address);
            }

            _bans.CreateServerBan(info);
            await RespondOk(context);

            _sawmill.Info($"Banned player {located.Username} ({located.UserId}) for {reason} lasting {body.Minutes ?? 0} minutes by {FormatLogActor(actor)}");
        });
    }

    /// <summary>
    ///     Kicks a player.
    /// </summary>
    private async Task ActionKick(IStatusHandlerContext context, Actor actor)
    {
        var body = await ReadJson<KickActionBody>(context);
        if (body == null)
            return;

        await RunOnMainThread(async () =>
        {
            if (!_playerManager.TryGetSessionById(new NetUserId(body.Guid), out var player))
            {
                await RespondError(
                    context,
                    ErrorCode.PlayerNotFound,
                    HttpStatusCode.UnprocessableContent,
                    "Player not found");
                return;
            }

            var reason = body.Reason ?? "No reason supplied";
            reason += " (kicked by admin)";

            _netManager.DisconnectChannel(player.Channel, reason);
            await RespondOk(context);

            _sawmill.Info($"Kicked player {player.Name} ({player.UserId}) for {reason} by {FormatLogActor(actor)}");
        });
    }

    private async Task ActionRoundStart(IStatusHandlerContext context, Actor actor)
    {
        await RunOnMainThread(async () =>
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();

            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                await RespondError(
                    context,
                    ErrorCode.InvalidRoundState,
                    HttpStatusCode.Conflict,
                    "Round already started");
                return;
            }

            ticker.StartRound();
            _sawmill.Info($"Forced round start by {FormatLogActor(actor)}");
            await RespondOk(context);
        });
    }

    private async Task ActionRoundEnd(IStatusHandlerContext context, Actor actor)
    {
        await RunOnMainThread(async () =>
        {
            var roundEndSystem = _entitySystemManager.GetEntitySystem<RoundEndSystem>();
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();

            if (ticker.RunLevel != GameRunLevel.InRound)
            {
                await RespondError(
                    context,
                    ErrorCode.InvalidRoundState,
                    HttpStatusCode.Conflict,
                    "Round is not active");
                return;
            }

            roundEndSystem.EndRound();
            _sawmill.Info($"Forced round end by {FormatLogActor(actor)}");
            await RespondOk(context);
        });
    }

    private async Task ActionRoundRestartNow(IStatusHandlerContext context, Actor actor)
    {
        await RunOnMainThread(async () =>
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();

            ticker.RestartRound();
            _sawmill.Info($"Forced instant round restart by {FormatLogActor(actor)}");
            await RespondOk(context);
        });
    }

    #endregion

    #region Fetching

    /// <summary>
    ///     Returns an array containing all available presets.
    /// </summary>
    private async Task GetPresets(IStatusHandlerContext context)
    {
        var presets = await RunOnMainThread(() =>
        {
            var presets = new List<PresetResponse.Preset>();

            foreach (var preset in _prototypeManager.EnumeratePrototypes<GamePresetPrototype>())
            {
                presets.Add(new PresetResponse.Preset
                {
                    Id = preset.ID,
                    ModeTitle = _loc.GetString(preset.ModeTitle),
                    Description = _loc.GetString(preset.Description)
                });
            }

            return presets;
        });

        await context.RespondJsonAsync(new PresetResponse
        {
            Presets = presets
        });
    }

    /// <summary>
    ///    Returns an array containing all game rules.
    /// </summary>
    private async Task GetGameRules(IStatusHandlerContext context)
    {
        var gameRules = new List<string>();
        foreach (var gameRule in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            if (gameRule.Abstract)
                continue;

            if (gameRule.HasComponent<GameRuleComponent>(_componentFactory))
                gameRules.Add(gameRule.ID);
        }

        await context.RespondJsonAsync(new GameruleResponse
        {
            GameRules = gameRules
        });
    }


    /// <summary>
    ///     Handles fetching information.
    /// </summary>
    private async Task InfoHandler(IStatusHandlerContext context, Actor actor)
    {
        /*
        Information to display
        Round number
        Connected players
        Active admins
        Active game rules
        Active game preset
        Active map
        MOTD
        Panic bunker status
        */

        var info = await RunOnMainThread<InfoResponse>(() =>
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();
            var adminSystem = _entitySystemManager.GetEntitySystem<AdminSystem>();

            var players = new List<InfoResponse.Player>();

            foreach (var player in _playerManager.Sessions)
            {
                var adminData = _adminManager.GetAdminData(player, true);

                players.Add(new InfoResponse.Player
                {
                    UserId = player.UserId.UserId,
                    Name = player.Name,
                    IsAdmin = adminData != null,
                    IsDeadminned = !adminData?.Active ?? false
                });
            }

            InfoResponse.MapInfo? mapInfo = null;
            if (_gameMapManager.GetSelectedMap() is { } mapPrototype)
            {
                mapInfo = new InfoResponse.MapInfo
                {
                    Id = mapPrototype.ID,
                    Name = mapPrototype.MapName
                };
            }

            var gameRules = new List<string>();
            foreach (var addedGameRule in ticker.GetActiveGameRules())
            {
                var meta = _entityManager.MetaQuery.GetComponent(addedGameRule);
                gameRules.Add(meta.EntityPrototype?.ID ?? meta.EntityPrototype?.Name ?? "Unknown");
            }

            var panicBunkerCVars = PanicBunkerCVars.ToDictionary(c => c, c => _config.GetCVar(c));
            return new InfoResponse
            {
                Players = players,
                RoundId = ticker.RoundId,
                Map = mapInfo,
                PanicBunker = panicBunkerCVars,
                GamePreset = ticker.CurrentPreset?.ID,
                GameRules = gameRules,
                MOTD = _config.GetCVar(CCVars.MOTD)
            };
        });

        await context.RespondJsonAsync(info);
    }

    #endregion

    private async Task<bool> CheckAccess(IStatusHandlerContext context)
    {
        var auth = context.RequestHeaders.TryGetValue("Authorization", out var authToken);
        if (!auth)
        {
            await RespondError(
                context,
                ErrorCode.AuthenticationNeeded,
                HttpStatusCode.Unauthorized,
                "Authorization is required");
            return false;
        }

        var authHeaderValue = authToken.ToString();
        var spaceIndex = authHeaderValue.IndexOf(' ');
        if (spaceIndex == -1)
        {
            await RespondBadRequest(context, "Invalid Authorization header value");
            return false;
        }

        var authScheme = authHeaderValue[..spaceIndex];
        var authValue = authHeaderValue[spaceIndex..].Trim();

        if (authScheme != SS14TokenScheme /*WL-Changes-start*/&& authScheme != WLAuthTokenScheme/*WL-Changes-end*/)
        {
            await RespondBadRequest(context, "Invalid Authorization scheme");
            return false;
        }

        //WL-Changes-start
        if (!string.IsNullOrEmpty(_corvax_token))
            if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(authValue),
                Encoding.UTF8.GetBytes(_corvax_token)))
                return true;

        if (!string.IsNullOrEmpty(_wl_token))
            if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(authValue),
                Encoding.UTF8.GetBytes(_wl_token)))
                return true;
        //WL-Changes-end

        await RespondError(
            context,
            ErrorCode.AuthenticationInvalid,
            HttpStatusCode.Unauthorized,
            "Authorization is invalid");

        // Invalid auth header, no access
        _sawmill.Info($"Unauthorized access attempt to admin API from {context.RemoteEndPoint}");
        return false;
    }

    private async Task<Actor?> CheckActor(IStatusHandlerContext context)
    {
        // The actor is JSON encoded in the header
        var actor = context.RequestHeaders.TryGetValue("Actor", out var actorHeader) ? actorHeader.ToString() : null;
        if (actor == null)
        {
            await RespondBadRequest(context, "Actor must be supplied");
            return null;
        }

        Actor? actorData;
        try
        {
            var innerActorData = JsonSerializer.Deserialize<InnerActor>(actor);
            if (innerActorData == null)
            {
                await RespondBadRequest(context, "Actor is null");
                return null;
            }

            //WL-Changes-start
            actorData = JsonSerializer.Deserialize<Actor>(actor);
            /*
            if (StuffBotIds.Contains(innerActorData.DiscordId))
            {
                return new Actor()
                {
                    DiscordId = innerActorData.DiscordId,
                    Record = new(new(Guid.Empty), DateTimeOffset.UnixEpoch, "STUFFBOT", DateTimeOffset.UtcNow, IPAddress.None, new([], HwidType.Modern)),
                    IsStuffBot = true
                };
            }

            var record = await _db.GetPlayerByDiscordId(innerActorData.DiscordId, default);
            if (record == null)
            {
                await RespondBadRequest(context, "Текущий дискорд-аккаунт не привязан к игровому аккаунту!");
                return null;
            }

            actorData = new() { Record = record, DiscordId = innerActorData.DiscordId, IsStuffBot = false };
            */
            //WL-Changes-end
        }
        catch (JsonException exception)
        {
            await RespondBadRequest(context, "Actor field JSON is invalid", ExceptionData.FromException(exception));
            return null;
        }

        return actorData;
    }

    //WL-Changes-start
    public async Task<bool> IsAdmin(PlayerRecord record, CancellationToken cancel = default)
    {
        return await IsAdmin(record.UserId, cancel);
    }

    public async Task<bool> IsAdmin(NetUserId user, CancellationToken cancel = default)
    {
        var data = await _db.GetAdminDataForAsync(user, cancel);
        if (data == null)
            return false;

        return true;
    }

    public async Task<bool> CheckAdminFlags(PlayerRecord record, AdminFlags query_flags, CancellationToken cancel = default)
    {
        return await CheckAdminFlags(record.UserId, query_flags, cancel);
    }

    public async Task<bool> CheckAdminFlags(NetUserId userId, AdminFlags query_flags, CancellationToken cancel = default)
    {
        var data = await _db.GetAdminDataForAsync(userId, cancel);
        if (data == null)
            return false;

        var exist_flags = AdminFlagsHelper.NamesToFlags(data.Flags.ToDictionary(k => k.Flag, v => v.Negative));

        return exist_flags.HasFlag(query_flags);
    }
    //WL-Changes-end

    #region From Client

    private sealed class Actor
    {
        public required Guid Guid { get; init; }
        public required string Name { get; init; }
        //WL-Changes-start
        public required PlayerRecord Record { get; init; }
        public required ulong DiscordId { get; init; }
        public required bool IsStuffBot { get; init; }
        //WL-Changes-end
    }

    private sealed class KickActionBody
    {
        public required Guid Guid { get; init; }
        public string? Reason { get; init; }
    }

    //WL-Changes-start
    private sealed class InnerActor
    {
        public required ulong DiscordId { get; init; }
    }

    private sealed class AhelpBody
    {
        public required string TargetUsername { get; init; }
        public required string Message { get; init; }
    }

    private sealed class LinkUserDiscordBody
    {
        public required string Login { get; init; }
        public required string Code { get; init; }
        public required ulong User { get; init; }
    }
    //WL-Changes-end

    private sealed class BanActionBody
    {
        public required Guid Guid { get; init; }
        public string? Reason { get; init; }
        public NoteSeverity Severity { get; init; }
        public uint? Minutes { get; init; }
    }

    private sealed class GameRuleActionBody
    {
        public required string GameRuleId { get; init; }
    }

    private sealed class PresetActionBody
    {
        public required string PresetId { get; init; }
    }

    private sealed class MotdActionBody
    {
        public required string Motd { get; init; }
    }

    #endregion

    #region Responses

    private record BaseResponse(
        string Message,
        ErrorCode ErrorCode = ErrorCode.None,
        ExceptionData? Exception = null);

    private record ExceptionData(string Message, string? StackTrace = null)
    {
        public static ExceptionData FromException(Exception e)
        {
            return new ExceptionData(e.Message, e.StackTrace);
        }
    }

    private enum ErrorCode
    {
        None = 0,
        AuthenticationNeeded = 1,
        AuthenticationInvalid = 2,
        InvalidRoundState = 3,
        PlayerNotFound = 4,
        GameRuleNotFound = 5,
        BadRequest = 6,
        PlayerAlreadyBanned = 7,
        //WL-Changes-start
        ServiceUnavailable = 8
        //WL-Changes-end
    }

    #endregion

    #region Misc

    /// <summary>
    /// Record used to send the response for the info endpoint.
    /// </summary>
    private sealed class InfoResponse
    {
        public required int RoundId { get; init; }
        public required List<Player> Players { get; init; }
        public required List<string> GameRules { get; init; }
        public required string? GamePreset { get; init; }
        public required MapInfo? Map { get; init; }
        public required string? MOTD { get; init; }
        public required Dictionary<string, object> PanicBunker { get; init; }

        public sealed class Player
        {
            public required Guid UserId { get; init; }
            public required string Name { get; init; }
            public required bool IsAdmin { get; init; }
            public required bool IsDeadminned { get; init; }
        }

        public sealed class MapInfo
        {
            public required string Id { get; init; }
            public required string Name { get; init; }
        }
    }

    private sealed class PresetResponse
    {
        public required List<Preset> Presets { get; init; }

        public sealed class Preset
        {
            public required string Id { get; init; }
            public required string Description { get; init; }
            public required string ModeTitle { get; init; }
        }
    }

    private sealed class GameruleResponse
    {
        public required List<string> GameRules { get; init; }
    }

    #endregion

    //WL-Changes-start
    private static class Constants
    {
        public const string PolyMapImage = "image";
    }
    //WL-Changes-end
}
