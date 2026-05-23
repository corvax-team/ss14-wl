using System.Text;

namespace Content.Shared._WL.Languages;

///TODO: Заменить списки строк на <see cref="Dataset.DatasetPrototype"/> по всему файлу?
///TODO: Расставить [Virtual], sealed или abstract.
///TODO: Убрать излишние аллокации: создание одинаковых списков каждый вызов <see cref="Obfuscate(StringBuilder, string, int)"/>.
///TODO: Привести DataField поля <see cref="Utf16ReplacementObfuscation"/> в нормальный вид.
///TODO: ИЗБАВИТЬСЯ ОТ МАГИЧЕСКИХ ЧИСЕЛ:sob: и этой сложной логики, мб вынести все в какие-нибудь отдельные методы.
[ImplicitDataDefinitionForInheritors]
public abstract partial class ObfuscationMethod
{
    public static readonly ObfuscationMethod Default = new ReplacementObfuscation
    {
        Replacement = new List<string> { "<?>" }
    };

    internal int PseudoRandom(int seed, int global_seed, int start, int end)
    {
        int result = 0;
        int gap = end - start + 1;
        result = seed ^ (global_seed * 127) + 1;
        result = System.Math.Abs((result + 619251) * 27644437);
        result %= gap;
        result += start;
        return result;
    }

    internal abstract void Obfuscate(StringBuilder builder, string message, int global_seed);

    public string Obfuscate(string message, int global_seed)
    {
        var builder = new StringBuilder();
        Obfuscate(builder, message, global_seed);
        return builder.ToString();
    }
}

public sealed partial class ReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        var index = PseudoRandom(message.GetHashCode(), global_seed, 0, Replacement.Count - 1);
        builder.Append(Replacement[index]);
    }
}

public sealed partial class WordsReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    [DataField]
    public List<char> Punctuation = new List<char>() { '.', ',', ';', ':', '!', '?' };

    internal bool IsPunct(char ch)
    {
        return Punctuation.Contains(ch);
    }

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        int buffer = 0;
        int counter = 0;
        const char eof = (char)0;
        for (int i = 0; i <= message.Length; i++)
        {
            var ch = i < message.Length ? message[i] : eof;
            if (IsPunct(ch) || ch == ' ' || ch == eof)
            {
                if (counter > 0)
                {
                    var index = PseudoRandom(buffer + i * counter, global_seed, 0, Replacement.Count - 1);
                    builder.Append(Replacement[index]);
                    buffer = 0;
                    counter = 0;
                }

                if (ch != eof)
                {
                    builder.Append(ch);
                }
            }
            else
            {
                buffer += Math.Abs(buffer * 41 + ch + 13);
                counter++;
            }
        }
    }
}

public sealed partial class Utf16ReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public int Utf16start = 61;

    [DataField(required: true)]
    public int Utf16end = 61;

    [DataField]
    public bool Randlength = true;

    [DataField]
    public int Minlength = 3;

    [DataField]
    public int Maxlength = 10;

    [DataField]
    public List<char> Punctuation = new List<char>() { '.', ',', ';', ':', '!', '?' };

    internal bool IsPunct(char ch)
    {
        return Punctuation.Contains(ch);
    }

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        var maxLen = Math.Max(Minlength, Maxlength);
        var minLen = Math.Max(1, Minlength);

        int buffer = 0;
        int counter = 0;
        const char eof = (char)0;
        for (int i = 0; i <= message.Length; i++)
        {
            var ch = i < message.Length ? message[i] : eof;
            if (IsPunct(ch) || ch == ' ' || ch == eof)
            {
                if (counter > 0)
                {
                    var length = Randlength ? PseudoRandom(buffer, global_seed, minLen, maxLen) : counter;
                    for (int j = 0; j <= length; j++)
                    {
                        var char_code = PseudoRandom(buffer * (j + 3) + (j + 1) * counter, global_seed, Utf16start, Utf16end);
                        builder.Append((char)char_code);
                    }

                    buffer = 0;
                    counter = 0;
                }

                if (ch != eof)
                {
                    builder.Append(ch);
                }
            }
            else
            {
                buffer += buffer * 41 + ch;
                counter++;
            }
        }
    }
}


public sealed partial class ByCharReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    [DataField]
    public bool Randlength = true;

    [DataField]
    public int Minlength = 3;

    [DataField]
    public int Maxlength = 10;

    [DataField]
    public List<char> Punctuation = new List<char>() { '.', ',', ';', ':', '!', '?' };

    internal bool IsPunct(char ch)
    {
        return Punctuation.Contains(ch);
    }

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        var maxLen = Math.Max(Minlength, Maxlength);
        var minLen = Math.Max(1, Minlength);

        int buffer = 0;
        int counter = 0;
        const char eof = (char)0;
        for (int i = 0; i <= message.Length; i++)
        {
            var ch = i < message.Length ? message[i] : eof;
            if (IsPunct(ch) || ch == ' ' || ch == eof)
            {
                if (counter > 0)
                {
                    var length = Randlength ? PseudoRandom(buffer, global_seed, minLen, maxLen) : counter;
                    for (int j = 0; j <= length; j++)
                    {
                        var index = PseudoRandom(buffer * (j + 3) + (j + 1) * counter, global_seed, 0, Replacement.Count - 1);
                        builder.Append(Replacement[index]);
                    }

                    buffer = 0;
                    counter = 0;
                }

                if (ch != eof)
                {
                    builder.Append(ch);
                }
            }
            else
            {
                buffer += buffer * 41 + ch;
                counter++;
            }
        }
    }
}

public sealed partial class LengthObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    [DataField]
    public int Min = 10;

    [DataField]
    public int Max = 1000;

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        var gap = Math.Max(1, Max - Min);
        var normalized = (double)(message.Length - Min) / gap;
        int index = (int)Math.Round(normalized * Replacement.Count);

        index = Math.Max(0, Math.Min(index, Replacement.Count - 1));
        builder.Append(Replacement[index]);
    }
}
