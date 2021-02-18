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
            else if (timeElapsed.Minutes > 0)
                timeString = timeElapsed.Minutes.ToString() + " minutes, " + timeElapsed.Seconds.ToString() + " seconds";
            else
                timeString = timeElapsed.Seconds.ToString() + " seconds";
            return timeString;
        }
    }
}