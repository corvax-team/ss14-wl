using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Server._WL.Speech.EntitySystems;

public sealed class VulpAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly (Regex Regex, string[] Replace)[] _replace =
    [
        (new Regex("р+"), new[] { "р", "рр" }),
        (new Regex("Р+"), ["Р", "РР"])
    ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VulpAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, VulpAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        foreach (var (regex, replace) in _replace)
        {
            regex.Replace(message, _random.Pick(replace));
        }

        args.Message = message;
    }
}
