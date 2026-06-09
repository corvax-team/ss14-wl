using System.Collections.Generic;
using Content.Server._WL.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._WL.Speech.EntitySystems
{
    public sealed class CischiAccentSystem : EntitySystem
    {
        private static readonly Dictionary<string, string> Replacements = new()
        {
            { "я", "йа" },
            { "Я", "ЙА" },
            { "е", "йэ" },
            { "Е", "ЙЭ" },
            { "ю", "йу" },
            { "Ю", "ЙУ" },
            { "ц", "тс" },
            { "Ц", "ТС" },
            { "щ", "шь" },
            { "Щ", "ШЬ" },
            { "ч", "дз" },
            { "Ч", "ДЗ" },
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<CischiAccentComponent, AccentGetEvent>(OnAccent);
        }

        private void OnAccent(EntityUid uid, CischiAccentComponent component, AccentGetEvent args)
        {
            var message = args.Message;

            foreach (var replacement in Replacements)
            {
                if (!message.Contains(replacement.Key))
                    continue;

                message = message.Replace(replacement.Key, replacement.Value);
            }

            args.Message = message;
        }
    }
}
