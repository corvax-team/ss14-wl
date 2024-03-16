using Content.Server._WL.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems
{
    public sealed class CischiAccentSystem : EntitySystem
    {

        public override void Initialize()
        {
            SubscribeLocalEvent<CischiAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {

            return message
                .Replace("я", "йа").Replace("Я", "ЙА")
                .Replace("е", "йэ").Replace("Е", "ЙЭ")
                .Replace("ю", "йу").Replace("Ю", "ЙУ")
                .Replace("ц", "тс").Replace("Ц", "ТС")
                .Replace("щ", "шь").Replace("Щ", "ШЬ")
                .Replace("ч", "дз").Replace("Ч", "ДЗ");
        }

        private void OnAccent(EntityUid uid, CischiAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
