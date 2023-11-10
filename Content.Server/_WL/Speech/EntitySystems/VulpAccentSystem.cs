using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems;

public sealed class VulpAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VulpAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, VulpAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = Regex.Replace(
            message,
            "р+",
            _random.Pick(new List<string>() { "р", "рр" })
        );

        message = Regex.Replace(
            message,
            "Р+",
            _random.Pick(new List<string>() { "Р", "РР" })
        );
        args.Message = message;
    }
}
