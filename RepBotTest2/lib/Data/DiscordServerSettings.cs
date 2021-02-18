using System;

namespace RepBot.lib.Data
{
    public class DiscordServerSettings
    {
        public string HardClearChannelID { get; set; }
        public string RepChannelID { get; set; }
        public int MaxRepAmount { get; set; } = 20;
        public TimeSpan RepTimeout { get; set; } = TimeSpan.FromHours(2);

    }
}