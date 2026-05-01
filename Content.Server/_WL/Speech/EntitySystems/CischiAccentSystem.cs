using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._WL.Speech.EntitySystems
{
    public sealed class CischiAccentSystem : EntitySystem
    {
        private static readonly Regex ReplacementsYa = new Regex("йа");
        private static readonly Regex ReplacementsYaUpper = new Regex("ЙА");
        private static readonly Regex ReplacementsYe = new Regex("йэ");
        private static readonly Regex ReplacementsYeUpper = new Regex("ЙЭ");
        private static readonly Regex ReplacementsYu = new Regex("йу");
        private static readonly Regex ReplacementsYuUpper = new Regex("ЙУ");
        private static readonly Regex ReplacementsC = new Regex("тс");
        private static readonly Regex ReplacementsCUpper = new Regex("ТС");
        private static readonly Regex ReplacementsSh = new Regex("шь");
        private static readonly Regex ReplacementsShUpper = new Regex("ШЬ");
        private static readonly Regex ReplacementsCh = new Regex("дз");
        private static readonly Regex ReplacementsChUpper = new Regex("ДЗ");

        public override void Initialize()
        {
            SubscribeLocalEvent<CischiAccentComponent, AccentGetEvent>(OnAccent);
        }

        private void OnAccent(EntityUid uid, CischiAccentComponent component, AccentGetEvent args)
        {
            var message = args.Message;

            message = ReplacementsYa.Replace(message, "я");
            message = ReplacementsYaUpper.Replace(message, "Я");
            message = ReplacementsYe.Replace(message, "е");
            message = ReplacementsYeUpper.Replace(message, "Е");
            message = ReplacementsYu.Replace(message, "ю");
            message = ReplacementsYuUpper.Replace(message, "Ю");
            message = ReplacementsC.Replace(message, "ц");
            message = ReplacementsCUpper.Replace(message, "Ц");
            message = ReplacementsSh.Replace(message, "щ");
            message = ReplacementsShUpper.Replace(message, "Щ");
            message = ReplacementsCh.Replace(message, "ч");
            message = ReplacementsChUpper.Replace(message, "Ч");

            args.Message = message;
        }
    }
}
