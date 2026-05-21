using System.Collections.Generic;
using System;


public static class TimeUtils
{
    public const int SecondsInDay = 86400;

    public static int GetDayOfYear()
    {
        return System.DateTime.UtcNow.DayOfYear;
    }

    public static string GetLocalizedDateByUnixTimestamp(int timestampSeconds)
    {
        var dt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        dt = dt.AddSeconds(timestampSeconds);
        return dt.ToShortDateString();
    }

    public static int GetUnixTimestamp()
    {
        return (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
    }

    public static double GetUnixTimestampMs()
    {
        return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
    }

    public static bool DidReach(int timestamp)
    {
        return GetUnixTimestamp() >= timestamp;
    }

    private static readonly Dictionary<string, (string s, string m, string h, string d)> units = new Dictionary<string, (string s, string m, string h, string d)>
    {
        { "en", ("s", "m", "h", "d") },
        { "fr", ("s", "min", "h", "j") },
        { "de", ("s", "m", "h", "T") },
        { "es", ("s", "min", "h", "d") },
        { "it", ("s", "m", "h", "g") },
        { "pt", ("s", "min", "h", "d") },
        { "ru", ("с", "м", "ч", "д") },
        { "pl", ("s", "m", "g", "d") },
        { "tr", ("sn", "dk", "sa", "g") },
        { "uk", ("с", "хв", "год", "д") },
        { "zh", ("秒", "分", "时", "天") },
        { "zh-Hant", ("秒", "分", "時", "天") },
        { "ja", ("秒", "分", "時", "日") },
        { "ko", ("초", "분", "시간", "일") },
    };

    public static string GetTimeLeft(int sec, string langCode)
    {
        if (!units.ContainsKey(langCode))
            langCode = "en";

        var u = units[langCode];

        if (sec < 60)
        {
            return sec + u.s;
        }
        if (sec < 60 * 60)
        {
            var min = sec / 60;
            var s = sec % 60;
            return s > 0 ? $"{min}{u.m} {s}{u.s}" : $"{min}{u.m}";
        }
        if (sec < 60 * 60 * 24)
        {
            var h = sec / (60 * 60);
            var min = (sec % (60 * 60)) / 60;
            return $"{h}{u.h} {min}{u.m}";
        }
        else
        {
            var d = sec / (60 * 60 * 24);
            var h = (sec % (60 * 60 * 24)) / (60 * 60);
            var min = (sec % (60 * 60)) / 60;
            return $"{d}{u.d} {h}{u.h} {min}{u.m}";
        }
    }

}
