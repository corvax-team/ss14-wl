using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems;

public sealed class KidanAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly (Regex Regex, string[] Replace)[] _replace =
    [
        (new Regex("с+"), new[] { "з", "зз" }),
        (new Regex("С+"), ["З", "ЗЗ"]),
        (new Regex("з+"), ["зз", "ззз"]),
        (new Regex("З+"), ["ЗЗ", "ЗЗЗ"]),
        (new Regex("ж+"), ["жж", "жжж"]),
        (new Regex("Ж+"), ["ЖЖ", "ЖЖЖ"])
    ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KidanAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, KidanAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        foreach (var (regex, replace) in _replace)
        {
            regex.Replace(message, _random.Pick(replace));
        }

        args.Message = message;
    }
}
