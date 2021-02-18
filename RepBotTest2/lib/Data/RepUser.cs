using RepBot.lib.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepBot.lib
{
    public struct RepUserInfo
    {
        public string Mention { get; set; }
        public string UsernameFull => $"{Username}#{Discriminator}";
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string NickName { get; set; }
        public string AvatarUrl { get; internal set; }

        internal static RepUserInfo GetInfo(Discord.WebSocket.SocketGuildUser user)
        {
            return new RepUserInfo()
            {
                Mention = user.Mention,
                Username = user.Username,
                Discriminator = user.Discriminator,
                NickName = user.Nickname ?? user.Username,
                AvatarUrl = user.GetAvatarUrl()
            };
        }
    }

    public class RepUser
    {
        public ulong DiscordUserId { get; set; }
        public List<Reputation> ReputationHistory { get; set; } = new List<Reputation>();
        public DateTime? LatestRepTime { get; set; } = null;
        public RepUserInfo? RepUserInfoCache { get; set; }

        public RepUser(Discord.WebSocket.SocketGuild guild, ulong discordUserId)
        {
            DiscordUserId = discordUserId;
            RepUserInfoCache = GetUserInfo(guild);
        }
        public RepUser() { } // empty ctor for json
        public int GetCurrentRep(RepType type = RepType.Total)
        {
            return ReputationHistory
                .Where(o => type == RepType.Total || (o.GoodRep && type == RepType.Positive) || (!o.GoodRep && type == RepType.Negative))
                .Sum(o => o.RepAmount);
        }

        public TimeSpan GetRepTimeout(TimeSpan serverTimeOut)
        {
            return serverTimeOut - (DateTime.UtcNow - LatestRepTime) ?? TimeSpan.FromSeconds(0);
        }
        public RepUserInfo GetUserInfo(Discord.WebSocket.SocketGuild guild)
        {
            var user = guild.GetUser(DiscordUserId);
            if (user == null)
            {
                if (RepUserInfoCache != null)
                {
                    return RepUserInfoCache.Value;
                }
                throw new Exception("User not found");
            }

            RepUserInfo info = RepUserInfo.GetInfo(user);
            RepUserInfoCache = info;
            return info;
        }

        public Reputation AddReputation(ulong RepId, RepUser giverUser, bool goodRep, string reason)
        {
            if(ReputationHistory.Find(o => o.UserId == giverUser.DiscordUserId && goodRep == goodRep) != null)
            {
        //        throw new Exception("You gave this person already reputation");
            }
            var rep = new Reputation(RepId, giverUser.DiscordUserId, goodRep, reason, giverUser.GetWeight());
            ReputationHistory.Add(rep);
            return rep;
        }

        private int GetWeight()
        {
            return 1;
        }

        public string GetAvatarUrl(Discord.WebSocket.SocketGuild guild)
        {
            var user = guild.GetUser(DiscordUserId);
            return user == null ? "https://i.imgur.com/R7mqXKL.png" : user.GetAvatarUrl();
        }
        public string GetReputationHistory(Discord.WebSocket.SocketGuild guild, DiscordServer server, int length = 5)
        {
            var reputationHistory = ReputationHistory.TakeLast(length).OrderByDescending(o => o.RepId).Select(o => $"{o.GetRepAmount()} from {server.GetRepUser(guild, o.UserId).GetUserInfo(guild).UsernameFull} ::: {o.Reason} ").ToList(); ;
            if (ReputationHistory.Count < length)
            {
                reputationHistory.Add("--- no history to display");
            }
            return string.Join("\n", reputationHistory);
        }


    }
}