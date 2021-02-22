using Discord;
using RepBot.lib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepBot.lib
{

    public class RepUser
    {
        public ulong DiscordUserId { get; set; }
        public ulong DiscordServerId { get; set; }
        public List<Reputation> ReputationHistory { get; set; } = new List<Reputation>();
        public DateTime? LatestRepTime { get; set; } = null;
        public RepUserInfo RepUserInfoCache { get; set; }
        protected DiscordServer server => DiscordServerStore.getInstance().GetServer(DiscordServerId);
        public bool HardClear { get; set; } = false;
        public RepUser(IGuild guild, ulong discordUserId)
        {
            DiscordUserId = discordUserId;
            DiscordServerId = guild.Id;
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
        public RepUserInfo GetUserInfo(IGuild guild)
        {
            var user = guild.GetUserAsync(DiscordUserId, mode: CacheMode.AllowDownload, options: new RequestOptions() { RetryMode = RetryMode.AlwaysRetry }).Result;
            if (user == null)
            {
                if (RepUserInfoCache != null)
                {
                    return RepUserInfoCache;
                }
                throw new Exception($"User not found in {guild.Name}");
            }

            RepUserInfo info = RepUserInfo.GetInfo(user);
            RepUserInfoCache = info;
            return info;
        }

        public Reputation AddReputation(ulong RepId, RepUser myUser, bool goodRep, string reason)
        {
            var rep = new Reputation(RepId, myUser.DiscordUserId, goodRep, reason, myUser.GetWeight());
            ReputationHistory.Add(rep);
            return rep;
        }

        public int GetWeight()
        {
            return HardClear ? 2 : 1;
        }

        public async Task UpdateHardCleared(IGuild guild)
        {
            var discordUser = await guild.GetUserAsync(DiscordUserId);
            if (discordUser == null)
            {
                return;
            }
            if (GetCurrentRep() >= server.Settings.HardClearAmount)
            {
                if (!discordUser.RoleIds.Contains(server.Settings.HardClearRoleId) || !HardClear)
                {
                    HardClear = true;
                    await discordUser.AddRoleAsync(guild.GetRole(server.Settings.HardClearRoleId));
                    DiscordServerStore.getInstance().Save();
                    var channel = await guild.GetTextChannelAsync(server.Settings.RepChannelID);
                    channel?.SendMessageAsync($":trophy: {RepUserInfoCache.Mention} | has been given the Hard Clear role. (they have met the requirement of {server.Settings.HardClearAmount} rep)");
                    await discordUser.SendMessageAsync($":trophy: **You have been given Hard Clear!** You have met the reputation requirement of {server.Settings.HardClearAmount}, congrats! Understand that this may be revoked at any time if you recieve enough negative reputation.)");
                }
            }
            else
            {
                if (discordUser.RoleIds.Contains(server.Settings.HardClearRoleId) || HardClear)
                {
                    HardClear = false;
                    await discordUser.RemoveRoleAsync(guild.GetRole(server.Settings.HardClearRoleId));
                    DiscordServerStore.getInstance().Save();
                    var channel = await guild.GetTextChannelAsync(server.Settings.RepChannelID);
                    channel?.SendMessageAsync($":no_entry_sign: {RepUserInfoCache.Mention} had Hard Clear revoked. (they have fallen below the requirement of {server.Settings.HardClearAmount} rep)");
                    await discordUser.SendMessageAsync($":no_entry_sign: **Frick your Hard Clear is gone**");

                }
                HardClear = false;
            }
        }

        public string GetAvatarUrl(IGuild guild)
        {
            var user = guild.GetUserAsync(DiscordUserId).Result;
            return user == null ? "https://i.imgur.com/R7mqXKL.png" : user.GetAvatarUrl();
        }
        public string GetReputationHistory(IGuild guild, int length = 5)
        {
            var reputationHistory = ReputationHistory.TakeLast(length).OrderByDescending(o => o.RepId).Select(o => o.ToHistoryString(server, guild)).ToList(); ;
            if (ReputationHistory.Count < length)
            {
                reputationHistory.Add("--- no history to display");
            }
            return string.Join("\n", reputationHistory);
        }


    }
}