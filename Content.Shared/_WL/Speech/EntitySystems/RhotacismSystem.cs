using System.Text.RegularExpressions;
using Content.Shared.Speech.Components;

namespace Content.Shared.Speech.EntitySystems;

public sealed partial class RhotacismSystem : RelayAccentSystem<RhotacismComponent>
{
    private static readonly Regex RegexUpperR = new("R");
    private static readonly Regex RegexLowerR = new("r");

    private static readonly Regex _regexLowerR = new Regex("р");
    private static readonly Regex _regexUpperR = new Regex("Р");
    public override string Accentuate(string message, Entity<RhotacismComponent>? ent = null)
    {
        message = RegexUpperR.Replace(message, "L");
        message = RegexLowerR.Replace(message, "l");

        message = _regexLowerR.Replace(message, "л");
        message = _regexUpperR.Replace(message, "Л");

        return message;
    }
}
