namespace StegoRevealer.Utils.DataPreparer.Lib;

public static class Helper
{
    public static string GetAssemblyDir()
    {
        string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        if (string.IsNullOrEmpty(path))
            path = AppContext.BaseDirectory;
        return path;
    }

    public static string GetFormattedElapsedTime(long milliseconds)
    {
        const long msInDay = 1000 * 60 * 60 * 24;
        const long msInHour = 1000 * 60 * 60;
        const long msInMinute = 1000 * 60;
        const long msInSecond = 1000;

        string result = string.Empty;

        long days = milliseconds / msInDay;
        if (days > 0)
        {
            result += $"{days} {FormattedDaysLabel(ReplaceAboveTenToCommonForFormatted(days))} ";
            milliseconds -= days * msInDay;
        }

        long hours = milliseconds / msInHour;
        if (hours > 0)
        {
            result += $"{hours} {FormattedHoursLabel(ReplaceAboveTenToCommonForFormatted(hours))} ";
            milliseconds -= hours * msInHour;
        }

        long minutes = milliseconds / msInMinute;
        if (minutes > 0)
        {
            result += $"{minutes} {FormattedMinutesLabel(ReplaceAboveTenToCommonForFormatted(minutes))} ";
            milliseconds -= minutes * msInMinute;
        }

        long seconds = milliseconds / msInSecond;
        if (seconds > 0)
        {
            result += $"{seconds} {FormattedSecondsLabel(ReplaceAboveTenToCommonForFormatted(seconds))} ";
            milliseconds -= seconds * msInSecond;
        }

        if (milliseconds > 0)
            result += $"{milliseconds} {FormattedMilisecondsLabel(ReplaceAboveTenToCommonForFormatted(milliseconds))} ";

        return result[0..^1];
    }

    private static long ReplaceAboveTenToCommonForFormatted(long val)
    {
        if (val is < 20 and > 10)
            return 0;
        return val;
    }

    #region Fomatted data strings

    private static string FormattedDaysLabel(long days) =>
        days.ToString()[^1] switch
        {
            '1' => "день",
            '2' => "дня",
            '3' => "дня",
            '4' => "дня",
            _ => "дней",
        };
    private static string FormattedHoursLabel(long days) =>
        days.ToString()[^1] switch
        {
            '1' => "час",
            '2' => "часа",
            '3' => "часа",
            '4' => "часа",
            _ => "часов",
        };
    private static string FormattedMinutesLabel(long days) =>
        days.ToString()[^1] switch
        {
            '1' => "минута",
            '2' => "минуты",
            '3' => "минуты",
            '4' => "минуты",
            _ => "минут",
        };
    private static string FormattedSecondsLabel(long days) =>
        days.ToString()[^1] switch
        {
            '1' => "секунда",
            '2' => "секунды",
            '3' => "секунды",
            '4' => "секунды",
            _ => "секунд",
        };
    private static string FormattedMilisecondsLabel(long days) =>
        days.ToString()[^1] switch
        {
            '1' => "милисекунда",
            '2' => "милисекунды",
            '3' => "милисекунды",
            '4' => "милисекунды",
            _ => "милисекунд",
        };

    #endregion
}
