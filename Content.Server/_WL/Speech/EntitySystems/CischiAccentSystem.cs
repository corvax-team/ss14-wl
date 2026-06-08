using System.Text.RegularExpressions;
using Content.Server._WL.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._WL.Speech.EntitySystems
{
    public sealed class CischiAccentSystem : EntitySystem
    {
        private static readonly Regex ReplacementsYa = new Regex("я");
        private static readonly Regex ReplacementsYaUpper = new Regex("Я");
        private static readonly Regex ReplacementsYe = new Regex("е");
        private static readonly Regex ReplacementsYeUpper = new Regex("Е");
        private static readonly Regex ReplacementsYu = new Regex("ю");
        private static readonly Regex ReplacementsYuUpper = new Regex("Ю");
        private static readonly Regex ReplacementsC = new Regex("ц");
        private static readonly Regex ReplacementsCUpper = new Regex("Ц");
        private static readonly Regex ReplacementsSh = new Regex("щ");
        private static readonly Regex ReplacementsShUpper = new Regex("Щ");
        private static readonly Regex ReplacementsCh = new Regex("ч");
        private static readonly Regex ReplacementsChUpper = new Regex("Ч");

        public override void Initialize()
        {
            SubscribeLocalEvent<CischiAccentComponent, AccentGetEvent>(OnAccent);
        }

        private void OnAccent(EntityUid uid, CischiAccentComponent component, AccentGetEvent args)
        {
            var message = args.Message;

            message = ReplacementsYa.Replace(message, "йа");
            message = ReplacementsYaUpper.Replace(message, "ЙА");
            message = ReplacementsYe.Replace(message, "йэ");
            message = ReplacementsYeUpper.Replace(message, "ЙЭ");
            message = ReplacementsYu.Replace(message, "йу");
            message = ReplacementsYuUpper.Replace(message, "ЙУ");
            message = ReplacementsC.Replace(message, "тс");
            message = ReplacementsCUpper.Replace(message, "ТС");
            message = ReplacementsSh.Replace(message, "шь");
            message = ReplacementsShUpper.Replace(message, "ШЬ");
            message = ReplacementsCh.Replace(message, "дз");
            message = ReplacementsChUpper.Replace(message, "ДЗ");

            args.Message = message;
        }
    }
}
