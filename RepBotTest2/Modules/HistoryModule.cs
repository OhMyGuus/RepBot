using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using RepBot.lib;
using RepBot.lib.Data;

namespace RepBot.Modules
{
    public class HistoryModule : BotModuleBase
    {
        public HistoryModule(ILogger<ReputationModule> logger) : base(logger) { }

        [Command("$me")]
        public async Task MyHistory()
        {
            RepUser repUser = GetRepUser(Context.User.Id.ToString());
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            await HistorySmall(repUser);
        }

        [Command("$history")]
        public async Task History(string userId = null, string mode = null)
        {
            if(userId == "me")
            {
                userId = null; //ugly but fine
            }
            RepUser repUser = GetRepUser(userId ?? Context.User.Id.ToString());
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            if(mode == "full")
            {
                await GetHistory(repUser, 10);
            }
            else
            {
                await HistorySmall(repUser);
            }
        }

        public async Task GetHistory(RepUser repUser, int amount = 10)
        {
            var requestUser = Context.User;
            var repCount = repUser.GetCurrentRep();
            bool goodRep = repCount >= 0;

            /// requested vars
            var repUserInfo = repUser.GetUserInfo(Context.Guild);
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(goodRep ? Color.Green : Color.Red);
            // builder.WithThumbnailUrl(repUser.GetAvatarUrl(Context.Guild));
            builder.AddField($":page_facing_up: Reputation history for {repUserInfo.NickName}", $"```diff\n{repUser.GetReputationHistory(Context.Guild, amount)}``` To view {repUserInfo.Username}'s reputation history, use `$history @{repUserInfo.Username} full`".WithMaxLength(1023));
            builder.AddField(":star2: Total Rep", $"```diff\n{ repUser.GetCurrentRep().ToString("+0;-#")}```", true);
            builder.AddField(":thumbsup: Positive Rep", $"```diff\n{ repUser.GetCurrentRep(RepType.Positive).ToString("+0;-#")}```", true);
            builder.AddField(":thumbsdown: Negative Rep", $"```diff\n{ repUser.GetCurrentRep(RepType.Negative).ToString("+0;-#")}```", true);

            builder.WithFooter($"Requested by {requestUser.Username}#{requestUser.Discriminator} | {repUserInfo.UsernameFull} ({repUser.DiscordUserId})", repUserInfo.AvatarUrl);
            await ReplyAsync(embed: builder.Build());
            await repUser.UpdateHardCleared(Context.Guild);
        }

        public async Task HistorySmall(RepUser repUser)
        {
            var requestUser = Context.User;
            var repCount = repUser.GetCurrentRep();
            bool goodRep = repCount >= 0;

            /// requested vars
            var repUserInfo = repUser.GetUserInfo(Context.Guild);
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(goodRep ? Color.Green : Color.Red);
            builder.WithThumbnailUrl(repUser.GetAvatarUrl(Context.Guild));
            builder.WithTitle($"{repUserInfo.NickName}'s Reputation Summary");
            builder.AddField(":star2: Reputation", $"```diff\n{ repUser.GetCurrentRep().ToString("+0;-#")} ({repUser.GetCurrentRep(RepType.Positive)}:{repUser.GetCurrentRep(RepType.Negative)})```", true);
            builder.AddField(":trophy: Hard Clear", repUser.HardClear? "```diff\nUnlocked :)```" : $"```diff\n{repUser.GetCurrentRep()}/{server.Settings.HardClearAmount}```", true);
            builder.AddField(":scales: Weight", $"```diff\n{ repUser.GetWeight()}```", true);

            builder.AddField($"Recent Reputation :page_facing_up:", $"```diff\n{repUser.GetReputationHistory(Context.Guild, 10)}``` To view {repUserInfo.Username}'s reputation history, use `$history @{repUserInfo.Username} full`");
            //builder.AddField(":star2: Total Rep", $"```diff\n{ repUser.GetCurrentRep().ToString("+0;-#")}```", true);
            //builder.AddField(":thumbsup: Positive Rep", $"```diff\n{ repUser.GetCurrentRep(RepType.Positive).ToString("+0;-#")}```", true);
            //builder.AddField(":thumbsdown: Negative Rep", $"```diff\n{ repUser.GetCurrentRep(RepType.Negative).ToString("+0;-#")}```", true);

            builder.WithFooter($"Requested by {requestUser.Username}#{requestUser.Discriminator} | {repUserInfo.UsernameFull} ({repUser.DiscordUserId})", requestUser.GetAvatarUrl());
            await ReplyAsync(embed: builder.Build());
            await repUser.UpdateHardCleared(Context.Guild);
        }
    }
}