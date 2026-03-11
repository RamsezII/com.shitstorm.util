using System;
using System.Diagnostics;
using UnityEngine;

public static partial class Util
{
    public static float DeltaTime => Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime;

    static readonly Stopwatch stopwatch = new();
    public static double TotalMilliseconds => stopwatch.Elapsed.TotalMilliseconds;

    //----------------------------------------------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    //----------------------------------------------------------------------------------------------------------

    public static string GetJSonTime()
    {
        DateTime now = DateTime.Now;
        return $"{now.Year:D4}-{now.Month:D2}-{now.Day:D2} - {now.Hour:D2}h{now.Minute:D2}";
    }

    public static string GetTime_hh_mm()
    {
        DateTime now = DateTime.Now;
        string time = "";

        if (now.Hour >= 10)
            time += now.Hour;
        else if (now.Hour != 0)
            time += "0" + now.Hour;
        else
            time += "00";

        time += ":";

        if (now.Minute >= 10)
            time += now.Minute;
        else if (now.Minute != 0)
            time += "0" + now.Minute;
        else
            time += "00";

        return time;
    }
}