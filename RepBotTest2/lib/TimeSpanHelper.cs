using System;

namespace RepBot.Modules
{
    public static class TimeSpanHelper
    {
        public static string GetHumanReadable(this TimeSpan timeElapsed)
        {
            string timeString;
            if (timeElapsed.Hours > 0)
                timeString = timeElapsed.Hours.ToString() + " hour(s), " + timeElapsed.Minutes.ToString() + " minutes, " + timeElapsed.Seconds.ToString() + " seconds";
            else if (timeElapsed.Minutes > 0 && timeElapsed.Seconds > 0)
                timeString = timeElapsed.Minutes.ToString() + " minutes, " + timeElapsed.Seconds.ToString() + " seconds";
            else if (timeElapsed.Minutes > 0 && timeElapsed.Seconds == 0)
                timeString = timeElapsed.Minutes.ToString() + " minutes";
            else
                timeString = timeElapsed.Seconds.ToString() + " seconds";
            return timeString;
        }

        public static string WithMaxLength(this string value, int maxLength)
        {
            return value?.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}