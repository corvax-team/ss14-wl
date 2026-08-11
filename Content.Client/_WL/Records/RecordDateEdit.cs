using System.Linq;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._WL.Records;

/// <summary>
/// A compact day-month-year editor used by structured character records.
/// </summary>
public sealed class RecordDateEdit : BoxContainer
{
    private string? _legacyValue;
    private bool _setting;

    public readonly LineEdit Day;
    public readonly LineEdit Month;
    public readonly LineEdit Year;

    public event Action? OnDateChanged;

    public string Text
    {
        get
        {
            if (_legacyValue != null)
                return _legacyValue;

            var day = Day.Text.Trim();
            var month = Month.Text.Trim();
            var year = Year.Text.Trim();
            return day.Length == 2 && month.Length == 2 && year.Length == 4
                ? $"{day}.{month}.{year}"
                : string.Empty;
        }
    }

    public RecordDateEdit()
    {
        Orientation = LayoutOrientation.Horizontal;

        Day = CreatePart("records-date-day", 45, 55);
        Month = CreatePart("records-date-month", 45, 55);
        Year = CreatePart("records-date-year", 75, 90);

        AddChild(Day);
        AddChild(CreateSeparator());
        AddChild(Month);
        AddChild(CreateSeparator());
        AddChild(Year);

        BindPart(Day, 2);
        BindPart(Month, 2);
        BindPart(Year, 4);
    }

    /// <summary>
    /// Loads either a structured date or an untouched legacy value.
    /// Legacy values remain intact until the user edits one of the date parts.
    /// </summary>
    public void SetText(string? value)
    {
        _setting = true;
        _legacyValue = null;

        try
        {
            var parts = value?.Split('.');
            if (parts is { Length: 3 } &&
                IsValidPart(parts[0], 2) &&
                IsValidPart(parts[1], 2) &&
                IsValidPart(parts[2], 4))
            {
                Day.Text = parts[0].Trim();
                Month.Text = parts[1].Trim();
                Year.Text = parts[2].Trim();
                return;
            }

            Day.Text = string.Empty;
            Month.Text = string.Empty;
            Year.Text = string.Empty;

            if (!string.IsNullOrWhiteSpace(value))
                _legacyValue = value;
        }
        finally
        {
            _setting = false;
        }
    }

    private void BindPart(LineEdit edit, int maxLength)
    {
        edit.OnTextChanged += _ =>
        {
            if (_setting)
                return;

            var sanitized = SanitizePart(edit.Text, maxLength);
            if (edit.Text != sanitized)
            {
                _setting = true;
                edit.Text = sanitized;
                edit.CursorPosition = Math.Min(edit.CursorPosition, sanitized.Length);
                _setting = false;
            }

            _legacyValue = null;
            OnDateChanged?.Invoke();
        };
    }

    private static LineEdit CreatePart(string placeholder, float minWidth, float maxWidth)
    {
        return new LineEdit
        {
            PlaceHolder = Loc.GetString(placeholder),
            MinWidth = minWidth,
            MaxWidth = maxWidth,
        };
    }

    private static Label CreateSeparator()
    {
        return new Label
        {
            Text = ".",
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(3, 0),
        };
    }

    private static string SanitizePart(string value, int maxLength)
    {
        return new string(value
            .Where(character => character is >= '0' and <= '9')
            .Take(maxLength)
            .ToArray());
    }

    private static bool IsValidPart(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length is > 0 &&
               trimmed.Length <= maxLength &&
               trimmed.All(character => character is >= '0' and <= '9');
    }
}
