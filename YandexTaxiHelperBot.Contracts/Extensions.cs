using System.Globalization;

namespace YandexTaxiHelperBot.Contracts;

public static class Extensions
{
    public static string ToStringWithPoint(this double number)
    {
        return number.ToString("G", CultureInfo.InvariantCulture);
    }
}