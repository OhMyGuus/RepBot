using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RepBot.db
{
    class Reputation
    {
        public ulong UserId;
        public string Reason;
        public int RepAmount;
        public bool GoodRep => RepAmount >= 0;
        public string GetRepAmount() => RepAmount.ToString("+0;-#");
        public Reputation(ulong userId, bool goodRep, string reason, int repWeight = 1)
        {
            UserId = userId;
            Reason = reason;
            RepAmount = goodRep? repWeight : repWeight * -1;
        }

        public static string GetUsername(Discord.WebSocket.SocketGuild guild, ulong userId)
        {
            var user = guild.GetUser(userId);
            return user == null ? userId.ToString() : $"{user.Username}#{user.Discriminator}";
        }
        public static string GetReputationHistory(Discord.WebSocket.SocketGuild guild, ulong userId, int length = 5)
        {

            var reputationHistory = tempRep.Where(o => o.UserId == userId).TakeLast(length).Select(o => $"{o.GetRepAmount()} from {GetUsername(guild, o.UserId)} ::: {o.Reason} ").ToList(); ;
            if (reputationHistory.Count < length)
            {
                reputationHistory.Add("--- no history to display");
            }
            return string.Join("\n", reputationHistory);
        }

        internal static int GetRepCount(ulong userId)
        {
            return tempRep.Where(o => o.UserId == userId).Sum(o => o.RepAmount);
        }

        public static List<Reputation> tempRep = new List<Reputation>();
    }


}
