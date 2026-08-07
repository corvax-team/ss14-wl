using System;
using Content.Shared._WL.Time;
using NUnit.Framework;

namespace Content.Tests.Shared._WL.Records;

[TestFixture]
public sealed class WLCalendarTest
{
    private static readonly DateTime LoreToday = new(2875, 8, 4);

    [TestCase(19, 1, 3, 2856)]
    [TestCase(19, 4, 8, 2856)]
    [TestCase(19, 20, 12, 2855)]
    public void BirthYearAccountsForWhetherBirthdayPassed(int age, int day, int month, int expectedYear)
    {
        Assert.That(WLCalendar.CalculateBirthYear(age, day, month, LoreToday), Is.EqualTo(expectedYear));
    }

    [Test]
    public void AgeAccountsForWhetherBirthdayPassed()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WLCalendar.CalculateAge(new DateTime(2856, 3, 1), LoreToday), Is.EqualTo(19));
            Assert.That(WLCalendar.CalculateAge(new DateTime(2855, 12, 20), LoreToday), Is.EqualTo(19));
        });
    }

    [Test]
    public void ImpossibleDateIsRejected()
    {
        Assert.That(WLCalendar.TryCreateBirthDate(19, 31, 2, LoreToday, out _), Is.False);
    }

    [Test]
    public void LeapDayUsesCalculatedBirthYear()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WLCalendar.TryCreateBirthDate(19, 29, 2, new DateTime(2875, 8, 4), out _), Is.True);
            Assert.That(WLCalendar.TryCreateBirthDate(18, 29, 2, new DateTime(2875, 8, 4), out _), Is.False);
        });
    }

    [Test]
    public void RecalculatingLeapBirthdayKeepsDateValid()
    {
        var recalculated = WLCalendar.RecalculateBirthDate(18, new DateTime(2856, 2, 29), LoreToday);

        Assert.Multiple(() =>
        {
            Assert.That(recalculated.Year, Is.EqualTo(2857));
            Assert.That(recalculated.Month, Is.EqualTo(2));
            Assert.That(recalculated.Day, Is.EqualTo(28));
        });
    }

    [TestCase("04.08.2856", 4, 8, 2856)]
    [TestCase("04/08/2856", 4, 8, 2856)]
    [TestCase("04-08-2856", 4, 8, 2856)]
    public void LegacySeparatorsAreAccepted(string value, int day, int month, int year)
    {
        Assert.That(WLCalendar.TryParseBirthDate(value, out var result), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Day, Is.EqualTo(day));
            Assert.That(result.Month, Is.EqualTo(month));
            Assert.That(result.Year, Is.EqualTo(year));
        });
    }
}
