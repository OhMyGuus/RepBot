using System;
using System.Data;
using System.Diagnostics;
using System.IO;
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
        public async Task Configure(int RepTimeOut, string logChannel, string repChannel, string hardclearRole, string adminRole, int maxRepAmount = 20, int hardClearAmount = 20)
        {
            ulong logchannelId = Context.Message.MentionedChannels.Skip(0).FirstOrDefault().Id;
            ulong repChannelId = Context.Message.MentionedChannels.Skip(1).FirstOrDefault().Id;
            ulong hardclearRoleId = Context.Message.MentionedRoles.Skip(0).FirstOrDefault().Id;
            ulong adminRoleId = Context.Message.MentionedRoles.Skip(1).FirstOrDefault().Id;

            DiscordServerStore.getInstance().ConfigureServer(Context.Guild.Id, new DiscordServerSettings()
            {
                RepTimeout = TimeSpan.FromSeconds(RepTimeOut),
                MaxRepAmount = maxRepAmount,
                LogChannelId = logchannelId,
                HardClearRoleId = hardclearRoleId,
                RepChannelID = repChannelId,
                HardClearAmount = hardClearAmount,
                AdminRoleId = adminRoleId
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
            sb.AppendLine($"{myUserInfo.UsernameFull} **removed** reputation from {repUser.InfoCache.UsernameFull}: ");
            sb.Append($"```diff\n{historyString}```");
            if (!string.IsNullOrEmpty(reason))
            {
                sb.Append($"with the reason: ```{reason}```");
            }
            await Log(sb.ToString());
        }

        [Command("=backup")]
        public async Task Backup()
        {
            await Context.Channel.SendFileAsync(new MemoryStream(Encoding.Default.GetBytes(DiscordServerStore.getInstance().ToJson())), "data.json");
        }

        [Command("=cleanup")]
        public async Task CleanUp()
        {
            var role = Context.Guild.GetRole(server.Settings.HardClearRoleId);
            await ReplyAsync($"U have 60 seconds to stop the bot before its done... removing  from{role.Name} with: {role.Members.Count()} member ");
            await Task.Delay(60000);
            foreach (var member in role.Members)
            {
                await member.RemoveRoleAsync(role);
                await ReplyAsync($":no_entry_sign: removed role from: {member.Username}");
            }
            await ReplyAsync("Done..");
        }

        [Command("=save")]
        public async Task Save()
        {
            DiscordServerStore.getInstance().Save();
            await ReplyAsync($"Made sure evreything was saved correctly");
        }


        [Command("=ping")]
        public async Task Ping()
        {
            var ping = (Context.Message.Timestamp.UtcDateTime - DateTimeOffset.UtcNow.UtcDateTime).TotalMilliseconds + " ms";

            await ReplyAsync($"Pong! -> :stopwatch: Message response latency: {ping} -> Discord api latency: {Context.Client.Latency} ");
        }


        [Command("=update")]
        public async Task Update()
        { 
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sudo",
                    Arguments = "nohup /updatebot.sh  &",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                }
            };
            process.Start();

            await ReplyAsync("Started update service1");
        }


    }
}