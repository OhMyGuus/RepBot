using Discord.Commands;
using Microsoft.Extensions.Logging;
using RepBot.lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepBot.Modules
{
    public class GeneralModule : BotModuleBase
    {
        public GeneralModule(ILogger<ReputationModule> logger) : base(logger) { }

        [Command("$time")]
        public async Task Time()
        {
            RepUser myUser = server.GetRepUser(Context.Guild, Context.User.Id);
            TimeSpan repTimeout = myUser.GetRepTimeout(server.Settings.RepTimeout);
            if (repTimeout.TotalSeconds > 0)
            {
                await ReplyAsync($":stopwatch: You need to wait {repTimeout.GetHumanReadable()}");//, maybe time to get some :cookie::milk:"); //
            }
            else
            {
                await ReplyAsync($":salad: Whohoo you can give someone rep again!");
            }
        }

        [Command("$playtime")]
        public async Task Playtime(string userId1 = "", string userId2 = "")
        {
            RepUser repUser = string.IsNullOrEmpty(userId2) ? server.GetRepUser(Context.Guild, Context.User.Id) : GetRepUser(userId1, 0);
            RepUser repUser2 = string.IsNullOrEmpty(userId2) ? (string.IsNullOrEmpty(userId1) ? repUser : GetRepUser(userId1, 0)) : GetRepUser(userId2, 1);

            if (repUser == null || repUser2 == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            var playtime = repUser.GetPlayTime(repUser2.DiscordUserId);
            if(repUser.DiscordUserId == repUser2.DiscordUserId)
            {
                await ReplyAsync($":alarm_clock:You played in total {playtime.GetHumanReadable()} ");
                return;
            }
            if (playtime.TotalSeconds == 0)
            {
                await ReplyAsync($":sweat_smile: No record of **{repUser.InfoCache.NickName}** and **{repUser2.InfoCache.NickName}** playing together");
                return;
            }
            await ReplyAsync($":stopwatch:**{repUser.InfoCache.NickName}** and **{repUser2.InfoCache.NickName}** played in total {playtime.GetHumanReadable()} together.");
        }

        [Command("$help")]
        public async Task Help()
        {
            await ReplyAsync($"I don't need help do u? (still in progress)");
        }

    }
}
