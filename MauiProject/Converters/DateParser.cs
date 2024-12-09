namespace MauiProject.Converters;
using System;
using System.Globalization;

public class DateParser
{
    public static DateTime ParseDate(string dateString)
    {

        string[] formats = {
            "yyyy-MM-dd HH:mm:ss",
            "ddd, MMM dd, yyyy hh:mm tt",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy/MM/dd hh:mm tt",
            "ddd, MMM dd, hh:mm",
            "ddd, MMM dd, HH:mm"
        };

        try
        {
            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }

            throw new FormatException($"Invalid date format: {dateString}");
        }
        catch (Exception ex)
        {
            throw new FormatException($"Failed to parse date: {dateString}. Error: {ex.Message}", ex);
        }
    }

}
