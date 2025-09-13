using System;
using System.Collections;
using System.Text;
using UnityEngine;

public static partial class Util
{
    public static string ToLog(this IEnumerable enumerable)
    {
        StringBuilder log = new("{ ");
        foreach (object item in enumerable)
            log.Append(item).Append(", ");
        log.Append("}");
        return log.ToString();
    }

    public static string TrimmedExceptionMessage(this Exception e) => e.Message.TrimEnd('\n', '\r', '\0');
    public static string FileSizeLog(this in uint fileSize) => ((long)fileSize).LogDataSize();
    public static string LogDataSize(this in long data_size)
    {
        if (data_size < 1024)
            return $"{data_size}B";
        else if (data_size < 1024 * 1024)
            return $"{Math.Round(data_size / 1024f, 2)}Ko";
        else if (data_size < 1024 * 1024 * 1024)
            return $"{Math.Round(data_size / (1024 * 1024f), 2)}Mo";
        else
            return $"{Math.Round(data_size / (1024 * 1024 * 1024f), 2)}Go";
    }

    public static string SecondsLog(this in long seconds)
    {
        if (seconds < 60)
            return $"{seconds}s";
        else if (seconds < 60 * 60)
            return $"{seconds / 60}m";
        else
            return $"{seconds / (60 * 60)}h";
    }

    public static string TimeLog(this float seconds, in bool forceMinutes = false, in bool forceHours = false)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)(seconds % 3600 / 60);
        int remainingSeconds = (int)(seconds % 60);

        string timeLog = "";
        if (hours > 0 || forceHours)
            timeLog += $"{hours}h ";
        if (minutes > 0 || forceMinutes)
        {
            timeLog += $"{minutes:D2}m ";
            timeLog += $"{remainingSeconds:D2}s";
        }
        else
            timeLog += $"{remainingSeconds}s";

        return timeLog;
    }


    public static string FullSecondsLog(this long seconds)
    {
        if (seconds < 60)
            return $"{seconds}s";
        else if (seconds < 60 * 60)
            return $"{seconds / 60}m {seconds % 60}s";
        else
            return $"{seconds / (60 * 60)}h {seconds / 60 % 60}m {seconds % 60}s";
    }

    public static string MillisecondsLog(this in double milliseconds)
    {
        if (milliseconds < 1000)
            return $"{Math.Round(milliseconds, 1)}ms";
        else if (milliseconds < 1000 * 60)
            return $"{Math.Round(milliseconds / 1000, 1)}s";
        else if (milliseconds < 1000 * 60 * 60)
            return $"{Math.Round(milliseconds / (1000 * 60), 1)}m";
        else
            return $"{Math.Round(milliseconds / (1000 * 60 * 60), 1)}h";
    }

    public static string PercentLog(this in float lerp, int digits = 0) => $"{Math.Round(lerp * 100, digits)}%";

    public static string ToProgressBar(this in float progress, in int width = 10, in int digits = 0)
    {
        int count = Mathf.RoundToInt(progress * width);
        return $"[{new string('=', count)}>{new string('-', width - count)}] {progress.PercentLog(digits)}";
    }
}