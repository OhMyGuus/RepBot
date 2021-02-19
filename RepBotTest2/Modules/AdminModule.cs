using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using RepBot.lib;
using RepBot.lib.Data;

namespace RepBot.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    public class AdminModule : BotModuleBase
    {
        public AdminModule(ILogger<ReputationModule> logger) : base(logger) { }


        [Command("=configure")]
        public async Task Configure(int RepTimeOut, string logChannel, string repChannel, string hardclearRole, int maxRepAmount = 20, int hardClearAmount = 20)
        {
            ulong logchannelId = Context.Message.MentionedChannels.FirstOrDefault().Id;
            ulong repChannelId = Context.Message.MentionedChannels.ToArray()[1].Id;
            ulong hardclearRoleId = Context.Message.MentionedRoles.FirstOrDefault().Id;
            DiscordServerStore.getInstance().ConfigureServer(Context.Guild.Id, new DiscordServerSettings()
            {
                RepTimeout = TimeSpan.FromSeconds(RepTimeOut),
                MaxRepAmount = maxRepAmount,
                LogChannelId = logchannelId,
                HardClearRoleId = hardclearRoleId,
                RepChannelID = repChannelId,
                HardClearAmount = hardClearAmount
            });
            await ReplyAsync("Configured server");
        }


        [Command("$reset")]
        public async Task Reset(string userid)
        {
            RepUser myUser = server.GetRepUser(Context.Guild, Context.User.Id);
            RepUser repUser = GetRepUser(userid);
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            repUser.ReputationHistory.Clear();
            DiscordServerStore.getInstance().Save();
            await ReplyAsync("Users Rep Cleared!");
        }

        [Command("$delete")]
        public async Task Delete(string userid, ulong RepID, [Remainder] string reason = null)
        {
            RepUser myUser = server.GetRepUser(Context.Guild, Context.User.Id);
            var myUserInfo = myUser.GetUserInfo(Context.Guild);
            RepUser repUser = GetRepUser(userid);
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            var reputation = repUser.ReputationHistory.Find(o => o.RepId == RepID);
            if (reputation == null)
            {
                await ReplyAsync("Reputation not found");
                return;
            }
            string historyString = reputation.ToHistoryString(server, Context.Guild);
            reputation.Delete(myUser);
            DiscordServerStore.getInstance().Save();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{myUserInfo.UsernameFull} **removed** reputation from {repUser.RepUserInfoCache.UsernameFull}: ");
            sb.Append($"```diff\n{historyString}```");
            if (!string.IsNullOrEmpty(reason))
            {
                sb.Append($"with the reason: ```{reason}```");
            }
            await Log(sb.ToString());
        }

    }
}