using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Microsoft.Extensions.Logging;
using RepBot.lib;
using RepBot.lib.Data;

namespace RepBot.Modules
{
    public class ReputationModule : BotModuleBase
    {
        public ReputationModule(ILogger<ReputationModule> logger) : base(logger) { }

        [Command("$time")]
        public async Task Time()
        {
            RepUser myUser = server.GetRepUser(Context.Guild, Context.User.Id);
            TimeSpan repTimeout = myUser.GetRepTimeout(server.Settings.RepTimeout);
            if (repTimeout.TotalSeconds > 0)
            {
                await ReplyAsync($":stopwatch: You need to wait {repTimeout.GetHumanReadable()}");
            }
            else
            {
                await ReplyAsync($":salad: Whohoo you can give someone rep again!");
            }
        }

        [Command("+rep")]
        public async Task PlusRep(string userid = "-1", [Remainder] string reason = "")
        {
            RepUser myUser = server.GetRepUser(Context.Guild, Context.User.Id);
            RepUser repUser = GetRepUser(userid);
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            await GiveRep(myUser, repUser, true, reason);
        }

        [Command("-rep", RunMode = RunMode.Async)]
        public async Task MinusRep(string userid = "-1", [Remainder] string reason = "") //[Remainder] string text
        {
            RepUser myUser = server.GetRepUser(Context.Guild, Context.User.Id);
            RepUser repUser = GetRepUser(userid);
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }

            await GiveMinusRep(myUser, repUser, reason);

        }

        public async Task GiveRep(RepUser myUser, RepUser repUser, bool goodRep, string reason)
        {
            if (!await CanGiveRep(myUser, repUser, goodRep, reason))
            {
                return;
            }
            var reputation = repUser.AddReputation(Context.Message.Id, myUser, goodRep, reason);
            myUser.LatestRepTime = DateTime.UtcNow;
            DiscordServerStore.getInstance().Save();
            var repCount = repUser.GetCurrentRep();

            /// requested vars
            var repUserInfo = repUser.GetUserInfo(Context.Guild);
            var myUserInfo = myUser.GetUserInfo(Context.Guild);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(goodRep ? Color.Green : Color.Red);
            builder.WithThumbnailUrl(repUserInfo.AvatarUrl);
            builder.WithTitle($":{(goodRep ? "thumbsup" : "thumbsdown")}: {repUserInfo.NickName}");
            builder.AddField(":star2: Total", $"```diff\n{repCount.ToString("+0;-#")}```", true);
            builder.AddField(":pencil2: Note", $"```{reason}```", true);
            builder.AddField("Recent Reputation :page_facing_up:", $"```diff\n{repUser.GetReputationHistory(Context.Guild, 5)}``` To view {repUserInfo.Username}'s reputation history, use `$history @{repUserInfo.Username}`");
            builder.WithFooter($"given by {myUserInfo.UsernameFull} | RepId: {repUser.DiscordUserId} {reputation.RepId}", myUserInfo.AvatarUrl);
            await ReplyAsync(embed: builder.Build());
            await repUser.UpdateHardCleared(Context.Guild);
            _logger.LogInformation($"{Context.User.Username} executed the ping command!");
            await LogRep(myUser, repUser, reputation);
        }

        public async Task GiveMinusRep(RepUser myUser, RepUser repUser, string reason)
        {
            if (!await CanGiveRep(myUser, repUser, false, reason))
            {
                return;
            }
            EmbedBuilder sb = new EmbedBuilder();
            sb.WithColor(Color.Red);
            sb.AddField("Confirm Negative Reputation",
                $"Are you sure you want to give {repUser.GetUserInfo(Context.Guild).Mention} negative reputation?" +
                " Understand that this should only be used against users who truly deserve it and abuse of this system will result in a punishment." +
                " If this player is actively breaking the rules, please create a ticket.");
            sb.AddField(":star2: Current Rep", $"```diff\n+{repUser.GetCurrentRep()} ({repUser.GetCurrentRep(lib.Data.RepType.Positive)}:{repUser.GetCurrentRep(lib.Data.RepType.Negative)})```", true);
            sb.AddField(":pencil2: Reason", $"```{reason}```", true);
            sb.WithFooter($"given by {myUser.GetUserInfo(Context.Guild).UsernameFull} | RepID: pending", Context.User.GetAvatarUrl());
            RestUserMessage message = (RestUserMessage)await ReplyAsync(embed: sb.Build());

            var noEmoji = new Emoji("❌");
            var yesEmoji = new Emoji("✔️");
            bool removed = false;
            await message.AddReactionsAsync(new Emoji[] { yesEmoji, noEmoji }); ;
            Func<Cacheable<IUserMessage, ulong>, Discord.WebSocket.ISocketMessageChannel, Discord.WebSocket.SocketReaction, Task> handler = null;
            handler = async (cachedMessage, channel, reaction) =>
                {
                    if (cachedMessage.HasValue && cachedMessage.Value.Id == message.Id && reaction.UserId == Context.User.Id)
                    {
                        if (reaction.Emote.Name == yesEmoji.Name)
                        {
                            try
                            {
                                await GiveRep(myUser, repUser, false, reason);
                            }
                            catch (Exception e)
                            {
                                await ReplyAsync(e.Message);
                            }
                            finally
                            {
                                await message.DeleteAsync();
                                removed = true;
                                Context.Client.ReactionAdded -= handler;
                            }
                        }
                        else if (reaction.Emote.Name == noEmoji.Name)
                        {
                            await message.DeleteAsync();
                            removed = true;
                            Context.Client.ReactionAdded -= handler;
                        }
                    }
                };

            Context.Client.ReactionAdded += handler;
            await Task.Delay(30000);
            if (!removed)
                await message.DeleteAsync();
        }

        public async Task<bool> CanGiveRep(RepUser myUser, RepUser repUser, bool goodRep, string reason)
        {
            TimeSpan repTimeout = myUser.GetRepTimeout(server.Settings.RepTimeout);
            if (repTimeout.TotalSeconds > 0)
            {
                await ReplyAsync($"You need to wait {repTimeout.GetHumanReadable()}");
                return false;
            }
            if (reason.Length < 5 || reason.Length > 100)
            {
                await ReplyAsync($"{myUser.RepUserInfoCache.Mention}, :x: Reason must be between 5 and 100 characters");
                return false;
            }
            if (repUser.GetCurrentRep() >= server.Settings.MaxRepAmount && goodRep)
            {
                await ReplyAsync($"User already has met his reputation limit");
                return false;
            }

            if (Context.Channel.Id != server.Settings.RepChannelID)
            {
                await ReplyAsync($"This is not the right channel");
                return false;
            }

            if (myUser.DiscordUserId == repUser.DiscordUserId)
            {
                await ReplyAsync($"Nice try buddy..");
                return false;
            }

            if (repUser.ReputationHistory.Find(o => o.UserId == myUser.DiscordUserId && o.GoodRep == goodRep && !o.Removed) != null)
            {
                await ReplyAsync("You gave this person already reputation");
                return false;
            }

            return true;
        }

        public async Task LogRep(RepUser myUser, RepUser repUser, Reputation rep)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{myUser.RepUserInfoCache.Mention} gave {rep.GetRepAmount()} to {repUser.RepUserInfoCache.Mention}");
            builder.Append($"```diff\n{rep.GetRepAmount()} [{rep.RepId}] {myUser.RepUserInfoCache.UsernameFull} \"{rep.Reason}\"```");
            builder.AppendLine($"*Remove this with* `$delete {repUser.DiscordUserId} {rep.RepId} [optional reason]`");
            await Log(builder.ToString());
        }

    }
}