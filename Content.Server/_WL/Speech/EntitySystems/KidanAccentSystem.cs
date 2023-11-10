using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems;

public sealed class KidanAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KidanAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, KidanAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = Regex.Replace(
            message,
            "с+",
            _random.Pick(new List<string>() { "з", "зз" })
        );

        message = Regex.Replace(
            message,
            "С+",
            _random.Pick(new List<string>() { "З", "ЗЗ" })
        );

        message = Regex.Replace(
            message,
            "з+",
            _random.Pick(new List<string>() { "зз", "ззз" })
        );

        message = Regex.Replace(
            message,
            "З+",
            _random.Pick(new List<string>() { "ЗЗ", "ЗЗЗ" })
        );

        message = Regex.Replace(
            message,
            "ж+",
            _random.Pick(new List<string>() { "жж", "жжж" })
        );

        message = Regex.Replace(
            message,
            "Ж+",
            _random.Pick(new List<string>() { "ЖЖ", "ЖЖЖ" })
        );
        args.Message = message;
    }
}
