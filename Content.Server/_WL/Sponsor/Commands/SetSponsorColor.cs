using Content.Server.Administration;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._WL.Administration.Commands
{
    [AdminCommand(AdminFlags.NameColor)]
    internal sealed class SetSponsorColor: LocalizedCommands
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IServerDbManager _dbManager = default!;
        [Dependency] private readonly IServerPreferencesManager _preferenceManager = default!;

        public override string Command => "setsponsorcolor";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {

            if (args.Length < 2)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!_playerManager.TryGetSessionByUsername(args[0], out var session))
            {
                shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
                return;
            }

            var color = Color.TryFromHex(args[1]);
            if (!color.HasValue)
            {
                shell.WriteError(Loc.GetString("shell-invalid-color-hex"));
                return;
            }

            // Save the DB
            _dbManager.SaveSponsorColorAsync(session.UserId, color.Value);
            // Update the cached preference
            var prefs = _preferenceManager.GetPreferences(session.UserId);
            prefs.SponsorColor = color.Value;
        }
    }
}
