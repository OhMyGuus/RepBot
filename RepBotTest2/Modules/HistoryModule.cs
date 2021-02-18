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
    public class HistoryModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ReputationModule> _logger;

        public HistoryModule(ILogger<ReputationModule> logger)
            => _logger = logger;

        private DiscordServer server => DiscordServerStore.getInstance().GetServer(Context.Guild.Id);
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

        [Command("$history")]
        public async Task History(string userId = null)
        {
            RepUser repUser = GetRepUser(userId ?? Context.User.Id.ToString());
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            await GetHistory(repUser);
        }

        public async Task GetHistory(RepUser repUser)
        {
            var requestUser = Context.User;
            var repCount = repUser.GetCurrentRep();
            bool goodRep = repCount >= 0;

            /// requested vars
            var repUserInfo = repUser.GetUserInfo(Context.Guild);
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(goodRep ? Color.Green : Color.Red);
            builder.WithThumbnailUrl(repUser.GetAvatarUrl(Context.Guild));
            builder.AddField($":page_facing_up: Reputation history for {repUserInfo.NickName}", $"```diff\n{repUser.GetReputationHistory(Context.Guild, server, 10)}``` To view {repUserInfo.Username}'s reputation history, use `$history @{repUserInfo.Username}`");
            builder.AddField(":star2: Total Rep", $"```diff\n{ repUser.GetCurrentRep().ToString("+0;-#")}```", true);
            builder.AddField(":thumbsup: Positive Rep", $"```diff\n{ repUser.GetCurrentRep(lib.Data.RepType.Positive).ToString("+0;-#")}```", true);
            builder.AddField(":thumbsdown: Negative Rep", $"```diff\n{ repUser.GetCurrentRep(lib.Data.RepType.Negative).ToString("+0;-#")}```", true);


            builder.WithFooter($"Requested by {requestUser.Username}#{requestUser.Discriminator} | {repUserInfo.UsernameFull} ({repUser.DiscordUserId})", requestUser.GetAvatarUrl());
            await ReplyAsync(embed: builder.Build());
            _logger.LogInformation($"{Context.User.Username} executed the ping command!");
        }


    }
}