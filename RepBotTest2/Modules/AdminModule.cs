using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using RepBot.db;
using RepBot.lib;
using RepBot.lib.Data;

namespace RepBot.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ReputationModule> _logger;

        public AdminModule(ILogger<ReputationModule> logger)
            => _logger = logger;

        private DiscordServer server => DiscordServerStore.getInstance().GetServer(Context.Guild.Id);
      

        [Command("=configure")]
        public async Task Configure(int RepTimeOut, int maxRepAmount = 20)
        {
            DiscordServerStore.getInstance().ConfigureServer(Context.Guild.Id, new DiscordServerSettings() { RepTimeout = TimeSpan.FromSeconds(RepTimeOut), MaxRepAmount = maxRepAmount });
            await ReplyAsync("Configured server");
        }
        [Command("$reset")]
        public async Task Reset(string userid)
        {
            RepUser giverUser = server.GetRepUser(Context.Guild, Context.User.Id);
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
        [Command("$remove")]
        public async Task Remove(string userid, ulong RepID, string reason)
        {
            RepUser giverUser = server.GetRepUser(Context.Guild, Context.User.Id);
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
            reputation.RepAmount = 0;
            reputation.Reason = $"removed by {giverUser.GetUserInfo(Context.Guild).UsernameFull}";
            DiscordServerStore.getInstance().Save();
            await ReplyAsync("Users Rep Cleared!");
        }
        private RepUser GetRepUser(string userId = null)
        {
            var userMention = Context.Message.MentionedUsers.FirstOrDefault();
            if (userMention != null)
            {
                return server.GetRepUser(Context.Guild, userMention.Id);
            }
            else
            {
                if (ulong.TryParse(userId, out ulong parsedId) && (Context.Guild.GetUser(parsedId) != null || server.GetRepUserOrNull(parsedId) != null))
                {
                    return server.GetRepUser(Context.Guild, parsedId);
                }
            }
            return null;
        }



    }
}