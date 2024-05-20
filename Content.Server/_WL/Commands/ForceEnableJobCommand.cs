using Content.Server.Administration;
using Content.Server.Preferences.Managers;
using Content.Shared._WL.Commands.Events;
using Content.Shared.Administration;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._WL.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed partial class ForceEnableJobCommand : LocalizedCommands
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IPlayerManager _playMan = default!;
        [Dependency] private readonly IServerPreferencesManager _prefMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;

        public override string Command => "forceenablejob";
        public override string Description => "Позволяет включить/выключить проверку на расу и возраст для должности определенного игрока.";
        public override string Help => $"forceenablejob <player> \"<character>\" <bool>";

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var names = _playMan.Sessions.OrderBy(c => c.Name).Select(c => c.Name);
                return CompletionResult.FromHintOptions(names, LocalizationManager.GetString("shell-argument-username-optional-hint"));
            }
            if (args.Length == 2)
            {
                if (!_playMan.TryGetUserId(args[0], out var netUserId))
                    return CompletionResult.Empty;

                var prefs = _prefMan.GetPreferencesOrNull(netUserId);
                if (prefs == null)
                    return CompletionResult.Empty;

                return CompletionResult.FromOptions(prefs.Characters.Select(c => c.Value.Name));
            }
            if (args.Length == 3)
            {
                var jobs = _protoMan.EnumeratePrototypes<JobPrototype>()
                    .OrderBy(j => j.ID)
                    .Select(j => j.ID);

                return CompletionResult.FromOptions(jobs);
            }
            if (args.Length == 4)
            {
                return CompletionResult.FromHintOptions(["True", "False"], "True/False");
            }

            return CompletionResult.Empty;
        }

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 4)
            {
                shell.WriteError(LocalizationManager.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!_playMan.TryGetSessionByUsername(args[0], out var session))
            {
                shell.WriteLine($"Игрока с никнеймом {args[0]} не существует!");
                return;
            }

            if (!_protoMan.TryIndex<JobPrototype>(args[2], out _))
            {
                shell.WriteLine($"Должности {args[2]} не существует!");
                return;
            }

            var profiles = _prefMan.GetPreferences(session.UserId).Characters
                .Where(c => c.Value.Name.Equals(args[1], StringComparison.CurrentCultureIgnoreCase))
                .Select(c => (c.Key, c.Value as HumanoidCharacterProfile
                    ?? throw new NotImplementedException($"{nameof(ForceEnableJobCommand)}: профиль не является HumanoidCharacterProfile")));

            var boolean = args[3] switch
            {
                "True" => true,
                "False" => false,
                _ => false
            };

            var forceEnableSys = _entities.System<ForceEnableJobSystem>();

            foreach (var profile in profiles)
            {
                var newProfile = profile.Item2.WithJobForcedEnable(args[2], boolean);
                forceEnableSys.Save(session, newProfile, profile.Key);
            }
        }
    }

    public sealed partial class ForceEnableJobSystem : EntitySystem
    {
        public void Save(ICommonSession session, ICharacterProfile profile, int slot)
        {
            var ev = new ServerSaveCharacterEvent(profile, slot);
            RaiseNetworkEvent(ev, session);
        }
    }
}
