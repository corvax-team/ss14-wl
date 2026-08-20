using System.Linq;
using System.Text;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Forensics;

/// <summary>
/// Builds safe paper markup for forensic scanner printouts.
/// </summary>
public static class ForensicReportFormatter
{
    private const string AccentColor = "#445F78";
    private const string AutomaticColor = "#252529";
    private const string ValueColor = "#55555D";
    private const string PlaceholderColor = "#85858C";

    public static string Format(
        string entityName,
        IEnumerable<string> fingerprints,
        IEnumerable<string> fibers,
        IEnumerable<string> touchDnas,
        IEnumerable<string> solutionDnas,
        IEnumerable<string> residues,
        Func<string, string> loc)
    {
        var builder = new StringBuilder();
        builder.Append("[head=3][color=").Append(AccentColor).Append(']')
            .Append(Escape(loc("forensic-scanner-printout-title")))
            .AppendLine("[/color][/head]");
        builder.Append("[bold]").Append(Escape(loc("forensic-scanner-printout-object")))
            .Append("[/bold] [color=").Append(AutomaticColor).Append(']')
            .Append(Escape(entityName)).AppendLine("[/color]");
        builder.AppendLine("[color=#77777D]────────────────────────[/color]");

        AppendSection(builder, loc("forensic-scanner-interface-fingerprints"), fingerprints, loc);
        AppendSection(builder, loc("forensic-scanner-interface-fibers"), fibers, loc);
        AppendSection(builder,
            loc("forensic-scanner-interface-dnas"),
            touchDnas.Concat(solutionDnas),
            loc);
        AppendSection(builder, loc("forensic-scanner-interface-residues"), residues, loc);

        return builder.ToString().TrimEnd();
    }

    private static void AppendSection(
        StringBuilder builder,
        string title,
        IEnumerable<string> values,
        Func<string, string> loc)
    {
        var distinctValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        builder.AppendLine();
        builder.Append("[color=").Append(AccentColor).Append("][bold]› ")
            .Append(Escape(title)).Append(" · ").Append(distinctValues.Count)
            .AppendLine("[/bold][/color]");

        if (distinctValues.Count == 0)
        {
            builder.Append("[color=").Append(PlaceholderColor).Append("][italic]")
                .Append(Escape(loc("forensic-scanner-printout-no-traces")))
                .AppendLine("[/italic][/color]");
            return;
        }

        foreach (var value in distinctValues)
        {
            builder.Append("[color=").Append(ValueColor).Append("]• ")
                .Append(Escape(value)).AppendLine("[/color]");
        }
    }

    private static string Escape(string value) => FormattedMessage.EscapeText(value);
}
