using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems;

public sealed class GolemAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GolemAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, GolemAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;
        if (_random.NextDouble() <= 0.4)
        {
            string[] words = message.Split(' ');

            Random random = new Random();
            int randomIndex = random.Next(0, words.Length);
            words[randomIndex] += "...";

            int randomSecondIndex = random.Next(0, words.Length);
            words[randomSecondIndex] += "...";

            args.Message = string.Join(" ", words);
        }
    }
}
