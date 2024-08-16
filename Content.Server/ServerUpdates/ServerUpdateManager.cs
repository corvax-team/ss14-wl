using System.Linq;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Server.Discord;
using Content.Shared.CCVar;
using Robust.Server;
using Robust.Server.Player;
using Robust.Server.ServerStatus;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.ServerUpdates;

/// <summary>
/// Responsible for restarting the server for update, when not disruptive.
/// </summary>
public sealed class ServerUpdateManager
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IWatchdogApi _watchdog = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IBaseServer _server = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    //WL-Chages-start
    [Dependency] private readonly DiscordWebhook _discord = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _log = default!;
    private WebhookIdentifier? _discordWebhook;
    //WL-Changes-end

    [ViewVariables]
    private bool _updateOnRoundEnd;

    private TimeSpan? _restartTime;

    public void Initialize()
    {
        _watchdog.UpdateReceived += WatchdogOnUpdateReceived;
        _playerManager.PlayerStatusChanged += PlayerManagerOnPlayerStatusChanged;

        //WL-Changes-start
        _log = _logManager.GetSawmill(nameof(ServerUpdateManager));

        var url = _cfg.GetCVar(CCVars.DiscordRoundUpdateWebhook);
        if (string.IsNullOrEmpty(url))
            return;

        _discord.GetWebhook(url, webhookData => _discordWebhook = webhookData.ToIdentifier());
        //WL-Changes-end
    }

    public void Update()
    {
        if (_restartTime != null && _restartTime < _gameTiming.RealTime)
        {
            DoShutdown();
        }
    }

    /// <summary>
    /// Notify that the round just ended, which is a great time to restart if necessary!
    /// </summary>
    /// <returns>True if the server is going to restart.</returns>
    public bool RoundEnded()
    {
        if (_updateOnRoundEnd)
        {
            DoShutdown();
            return true;
        }

        return false;
    }

    private void PlayerManagerOnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        switch (e.NewStatus)
        {
            case SessionStatus.Connecting:
                _restartTime = null;
                break;
            case SessionStatus.Disconnected:
                ServerEmptyUpdateRestartCheck();
                break;
        }
    }

    private async void WatchdogOnUpdateReceived()
    {
        _chatManager.DispatchServerAnnouncement(Loc.GetString("server-updates-received"));
        _updateOnRoundEnd = true;
        ServerEmptyUpdateRestartCheck();

        //WL-Changes-start
        await SendDiscordNotify();
        //WL-Changes-end
    }

    //WL-Changes-start
    private async Task SendDiscordNotify()
    {
        try
        {
            if (_discordWebhook == null)
                return;

            var payload = new WebhookPayload()
            {
                Content = "Сервер получил обновление и будет перезапущен в конце текущего раунда."
            };

            await _discord.CreateMessage(_discordWebhook.Value, payload);
        }
        catch (Exception exc)
        {
            _log.Error($"Вызвано исключение во время отправки дискорд-оповещение об обновлении сервера: {exc}");
        }
    }
    //WL-Changes-end

    /// <summary>
    ///     Checks whether there are still players on the server,
    /// and if not starts a timer to automatically reboot the server if an update is available.
    /// </summary>
    private void ServerEmptyUpdateRestartCheck()
    {
        // Can't simple check the current connected player count since that doesn't update
        // before PlayerStatusChanged gets fired.
        // So in the disconnect handler we'd still see a single player otherwise.
        var playersOnline = _playerManager.Sessions.Any(p => p.Status != SessionStatus.Disconnected);
        if (playersOnline || !_updateOnRoundEnd)
        {
            // Still somebody online.
            return;
        }

        if (_restartTime != null)
        {
            // Do nothing because I guess we already have a timer running..?
            return;
        }

        var restartDelay = TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.UpdateRestartDelay));
        _restartTime = restartDelay + _gameTiming.RealTime;
    }

    private void DoShutdown()
    {
        _server.Shutdown(Loc.GetString("server-updates-shutdown"));
    }
}
