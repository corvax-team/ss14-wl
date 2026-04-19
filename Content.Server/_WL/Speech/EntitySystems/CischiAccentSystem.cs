using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems
{
    public sealed class CischiAccentSystem : EntitySystem
    {
        private static readonly Regex _replacementsYa = new Regex("йа");
        private static readonly Regex _replacementsYaUpper = new Regex("ЙА");
        private static readonly Regex _replacementsYe = new Regex("йэ");
        private static readonly Regex _replacementsYeUpper = new Regex("ЙЭ");
        private static readonly Regex _replacementsYu = new Regex("йу");
        private static readonly Regex _replacementsYuUpper = new Regex("ЙУ");
        private static readonly Regex _replacementsC = new Regex("тс");
        private static readonly Regex _replacementsCUpper = new Regex("ТС");
        private static readonly Regex _replacementsSh = new Regex("шь");
        private static readonly Regex _replacementsShUpper = new Regex("ШЬ");
        private static readonly Regex _replacementsCh = new Regex("дз");
        private static readonly Regex _replacementsChUpper = new Regex("ДЗ");

        public override void Initialize()
        {
            SubscribeLocalEvent<CischiAccentComponent, AccentGetEvent>(OnAccent);
        }

        private void OnAccent(EntityUid uid, CischiAccentComponent component, AccentGetEvent args)
        {
            var message = args.Message;

            message = _replacementsYa.Replace(message, "я");
            message = _replacementsYaUpper.Replace(message, "Я");
            message = _replacementsYe.Replace(message, "е");
            message = _replacementsYeUpper.Replace(message, "Е");
            message = _replacementsYu.Replace(message, "ю");
            message = _replacementsYuUpper.Replace(message, "Ю");
            message = _replacementsC.Replace(message, "ц");
            message = _replacementsCUpper.Replace(message, "Ц");
            message = _replacementsSh.Replace(message, "щ");
            message = _replacementsShUpper.Replace(message, "Щ");
            message = _replacementsCh.Replace(message, "ч");
            message = _replacementsChUpper.Replace(message, "Ч");

            args.Message = message;
        }
    }
}
