using Content.Server._WL.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._WL.Speech.EntitySystems
{
    public sealed class CischiAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        private static readonly IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>()
        {
            { "you", "wu" },
            {"голем", "дадеоки"},
            {"голема", "дадеокинну"},
            {"голему", "дадеокид"},
            {"големом", "дадеокоз"},
            {"големе", "дадеокиз"},
            {"големы", "дадео"},
            {"големов", "даденну"},
            {"големам", "дадеод"},
            {"големами", "дадеоз"},
            {"големах", "дадез"},
            {"големский", "даде"},
            {"табак", "йака"},
            {"табаки", "йака"},
            {"табака", "йаканну"},
            {"табаку", "йакад"},
            {"табаком", "йаказ"},
            {"табаке", "йакз"},
            {"табачный", "йак"},
            {"табадзный", "йак"},
            {"дракон", "Му Лонг"},
            {"дракона", "Му Лонну"},
            {"дракону", "Му Лонд"},
            {"драконом", "Му Лонгоз"},
            {"драконе", "Му Лонгиз"},
            {"драконы", "Му Лон"},
            {"драконов", "Му Лонну"},
            {"драконам", "Му Лод"},
            {"драконами", "Му Лонз"},
            {"драконах", "Му Лоз"},
            {"бог", "Му Лонг"},
            {"бога", "Му Лонну"},
            {"богу", "Му Лонд"},
            {"богом", "Му Лонгоз"},
            {"боге", "Му Лонгиз"},
            {"боги", "Му Лон"},
            {"богов", "Му Лонну"},
            {"богам", "Му Лод"},
            {"богами", "Му Лонз"},
            {"богах", "Му Лоз"}
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<CischiAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            foreach (var (word, repl) in SpecialWords)
            {
                message = message.Replace(word, repl);
            }

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
