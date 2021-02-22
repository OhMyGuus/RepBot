using Discord;
using RepBot.lib.Data;
using System;

namespace RepBot.lib
{
    public class Reputation
    {
        public ulong RepId { get; set; }
        public ulong UserId { get; set; }
        public string Reason { get; set; }
        public int RepAmount { get; set; }
        public DateTime dateTime { get; set; } = DateTime.UtcNow;
        public bool GoodRep => RepAmount >= 0;
        public bool Removed { get; set; }
        public string GetRepAmount() => RepAmount == 0 ? " 0" : RepAmount.ToString("+#;-#;0");
        public Reputation(ulong repId, ulong userId, bool goodRep, string reason, int repWeight = 1)
        {
            RepId = repId;
            UserId = userId;
            Reason = reason;
            RepAmount = goodRep ? repWeight : repWeight * -1;
        }

        public void Delete(RepUser myUser)
        {
            Removed = true;
            UserId = myUser.DiscordUserId;
            RepAmount = 0;
            Reason = $"removed by {myUser.InfoCache.UsernameFull}";
        }

        public string ToHistoryString(DiscordServer server, IGuild guild)
        {
            return $"{GetRepAmount()} from {server.GetRepUser(guild, UserId).GetUserInfo(guild).UsernameFull} ::: {Reason}";
        }


    }
}