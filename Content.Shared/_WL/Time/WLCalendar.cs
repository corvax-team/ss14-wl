namespace Content.Shared._WL.Time;

/// <summary>
/// Gregorian calendar used by WL. Its day and month follow the real UTC date,
/// while its year is shifted to match the setting.
/// </summary>
public static class WLCalendar
{
    public const int YearOffset = 849;

    public static DateTime Today => GetLoreDate(DateTime.UtcNow.Date);

    public static DateTime GetLoreDate(DateTime realDate)
    {
        var year = realDate.Year + YearOffset;
        var day = Math.Min(realDate.Day, DateTime.DaysInMonth(year, realDate.Month));
        return new DateTime(year, realDate.Month, day);
    }

    public static int CalculateBirthYear(int age, int day, int month)
    {
        return CalculateBirthYear(age, day, month, Today);
    }

    public static int CalculateBirthYear(int age, int day, int month, DateTime today)
    {
        var birthdayPassed = month < today.Month ||
                             month == today.Month && day <= today.Day;
        return today.Year - age - (birthdayPassed ? 0 : 1);
    }

    public static bool TryCreateBirthDate(int age, int day, int month, out DateTime birthDate)
    {
        return TryCreateBirthDate(age, day, month, Today, out birthDate);
    }

    public static bool TryCreateBirthDate(int age, int day, int month, DateTime today, out DateTime birthDate)
    {
        birthDate = default;
        if (age < 0 || month is < 1 or > 12)
            return false;

        var year = CalculateBirthYear(age, day, month, today);
        if (year is < 1 or > 9999 || day < 1 || day > DateTime.DaysInMonth(year, month))
            return false;

        birthDate = new DateTime(year, month, day);
        return true;
    }

    public static DateTime RecalculateBirthDate(int age, DateTime previousBirthDate)
    {
        return RecalculateBirthDate(age, previousBirthDate, Today);
    }

    public static DateTime RecalculateBirthDate(int age, DateTime previousBirthDate, DateTime today)
    {
        var year = CalculateBirthYear(age, previousBirthDate.Day, previousBirthDate.Month, today);
        var day = Math.Min(previousBirthDate.Day, DateTime.DaysInMonth(year, previousBirthDate.Month));
        return new DateTime(year, previousBirthDate.Month, day);
    }

    public static int CalculateAge(DateTime birthDate)
    {
        return CalculateAge(birthDate, Today);
    }

    public static int CalculateAge(DateTime birthDate, DateTime today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate.Month > today.Month ||
            birthDate.Month == today.Month && birthDate.Day > today.Day)
        {
            age--;
        }

        return age;
    }

    public static bool TryParseBirthDate(string? value, out DateTime birthDate)
    {
        birthDate = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Trim().Split(new[] { '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var day) ||
            !int.TryParse(parts[1], out var month) ||
            !int.TryParse(parts[2], out var year) ||
            year is < 1 or > 9999 ||
            month is < 1 or > 12 ||
            day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        birthDate = new DateTime(year, month, day);
        return true;
    }

    public static string FormatBirthDate(DateTime birthDate)
    {
        return $"{birthDate.Day:00}.{birthDate.Month:00}.{birthDate.Year:0000}";
    }
}
