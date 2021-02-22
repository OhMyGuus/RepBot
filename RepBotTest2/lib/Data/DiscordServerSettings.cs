using System;

namespace RepBot.lib.Data
{
    public class DiscordServerSettings
    {
        public ulong RepChannelID { get; set; }
        public int MaxRepAmount { get; set; } = 20;
        public TimeSpan RepTimeout { get; set; } = TimeSpan.FromHours(2);
        public ulong LogChannelId { get;  set; }
        public int HardClearAmount { get; set; } = 10;
        public ulong HardClearRoleId { get;  set; }
        public ulong AdminRoleId { get;  set; }
    }
}