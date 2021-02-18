using System;
using System.Collections.Generic;
using System.Text;

namespace RepBot.lib.Data
{
    public class DiscordServer
    {    

        public ulong DiscordServerID { get; set; }
        public Dictionary<ulong, RepUser> RepUsers { get; set; } = new Dictionary<ulong, RepUser>();
        public DiscordServerSettings Settings { get;  set; }

        public DiscordServer(ulong discordServerID, DiscordServerSettings settings)
        {
            DiscordServerID = discordServerID;
            Settings = settings;
        }
        public DiscordServer(){ }

        public RepUser GetRepUser(Discord.WebSocket.SocketGuild guild, ulong userId)
        {
            if (!RepUsers.ContainsKey(userId))
            {
                RepUser newUser = new RepUser(guild, userId);
                RepUsers.Add(userId, newUser);
            }
            return RepUsers[userId];
        }

        internal RepUser GetRepUserOrNull(ulong userId)
        {
            if (!RepUsers.ContainsKey(userId))
            {
                return null;
            }
            return RepUsers[userId];
        }
    }
}
