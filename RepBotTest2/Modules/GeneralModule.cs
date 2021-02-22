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
                await ReplyAsync($":stopwatch: You need to wait {repTimeout.GetHumanReadable()}, maybe time to get some milk:chocolate_bar: :milk: ");
            }
            else
            {
                await ReplyAsync($":salad: Whohoo you can give someone rep again!");
            }
        }

        [Command("$playtime")]
        public async Task Playtime(string userId1, string userId2)
        {
            RepUser repUser = string.IsNullOrEmpty(userId2) ? server.GetRepUser(Context.Guild, Context.User.Id) : GetRepUser(userId1, 0);
            RepUser repUser2 = string.IsNullOrEmpty(userId2) ? GetRepUser(userId1, 0) : GetRepUser(userId2, 1);

            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }

            await ReplyAsync($":boom:Player {repUser.InfoCache.UsernameFull} and {repUser2.InfoCache.UsernameFull} played in total {repUser.GetPlayTime(repUser2.DiscordUserId).GetHumanReadable()}");
        }

        [Command("$help")]
        public async Task Help()
        {
            await ReplyAsync($"");
        }

    }
}
