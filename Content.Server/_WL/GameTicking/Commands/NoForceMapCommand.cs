using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;


namespace Content.Server._WL.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    sealed class NoForceMapCommand : IConsoleCommand
    {
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;

        public string Command => "noforcemap";
        public string Description => Loc.GetString("Убирает карту, которая была выставлена forcemap");
        public string Help => string.Empty;

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _configurationManager.SetCVar(CCVars.GameMap, string.Empty);
            shell.WriteLine(Loc.GetString("Очередь карт была очищена"));
        }
    }
}
