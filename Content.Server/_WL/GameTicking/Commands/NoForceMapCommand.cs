using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;


namespace Content.Server._WL.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    sealed partial class NoForceMapCommand : LocalizedCommands
    {
        [Dependency] private IConfigurationManager _configurationManager = default!;

        public override string Command => "noforcemap";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _configurationManager.SetCVar(CCVars.GameMap, string.Empty);
            shell.WriteLine(Loc.GetString("cmd-noforcemap-success"));
        }
    }
}
