using System;

namespace RepBot.lib.Data
{
    public class DiscordServerSettings
    {
        public ulong HardClearChannelID { get; set; }
        public ulong RepChannelID { get; set; }
        public int MaxRepAmount { get; set; } = 20;
        public TimeSpan RepTimeout { get; set; } = TimeSpan.FromHours(2);

    }
}