using System;
using System.Collections.Generic;
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
    public class ReputationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ReputationModule> _logger;

        public ReputationModule(ILogger<ReputationModule> logger)
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

        [Command("+rep")]
        public async Task PlusRep(string userid, [Remainder] string reason)
        {
            RepUser giverUser = server.GetRepUser(Context.Guild, Context.User.Id);
            RepUser repUser = GetRepUser(userid);
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }
            await GiveRep(giverUser, repUser, true, reason);
        }

        [Command("-rep", RunMode = RunMode.Async)]
        public async Task MinusRep(string userid, [Remainder] string reason) //[Remainder] string text
        {
            RepUser giverUser = server.GetRepUser(Context.Guild, Context.User.Id);
            RepUser repUser = GetRepUser(userid);
            if (repUser == null)
            {
                await ReplyAsync("Cannot find user");
                return;
            }

            await GiveMinusRep(giverUser, repUser, reason);

        }



        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Discord.WebSocket.ISocketMessageChannel arg2, Discord.WebSocket.SocketReaction arg3)
        {
            throw new NotImplementedException();
        }

        public async Task GiveRep(RepUser giverUser, RepUser repUser, bool goodRep, string reason)
        {
            TimeSpan repTimeout = giverUser.GetRepTimeout(server.Settings.RepTimeout);
            if (repTimeout.TotalSeconds > 0)
            {
                await ReplyAsync($"You need to wait {repTimeout.GetHumanReadable()}");
                return;
            }
            if (repUser.GetCurrentRep() >= server.Settings.MaxRepAmount)
            {
                await ReplyAsync($"User already has met his reputation limit");
                return;
            }

            var reputation = repUser.AddReputation(Context.Message.Id, giverUser, goodRep, reason);
            giverUser.LatestRepTime = DateTime.UtcNow;
            DiscordServerStore.getInstance().Save();
            var repCount = repUser.GetCurrentRep();
            
            /// requested vars
            var repUserInfo = repUser.GetUserInfo(Context.Guild);
            var giverUserInfo = giverUser.GetUserInfo(Context.Guild);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(goodRep ? Color.Green : Color.Red);
            builder.WithThumbnailUrl(repUserInfo.AvatarUrl);
            builder.WithTitle($":{(goodRep ? "thumbsup" : "thumbsdown")}: {repUserInfo.NickName}");
            builder.AddField(":star2: Total", $"```diff\n{repCount.ToString("+0;-#")}```", true);
            builder.AddField(":pencil2: Note", $"```{reason}```", true);
            builder.AddField("Recent Reputation :page_facing_up:", $"```diff\n{repUser.GetReputationHistory(Context.Guild, server, 5)}``` To view {repUserInfo.Username}'s reputation history, use `$history @{repUserInfo.Username}`");
            builder.WithFooter($"given by {giverUserInfo.UsernameFull} | RepId: {repUser.DiscordUserId} {reputation.RepId}", giverUserInfo.AvatarUrl);
            await ReplyAsync(embed: builder.Build());

            _logger.LogInformation($"{Context.User.Username} executed the ping command!");
            await LogRep(reputation);
        }



        public async Task GiveMinusRep(RepUser giveUser, RepUser repUser, string reason)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithColor(Color.Red);
            builder.AddField("Confirm Negative Reputation",
                $"Are you sure you want to give @Stumper negative reputation?" +
                " Understand that this should only be used against users who truly deserve it and abuse of this system will result in a punishment." +
                " If this player is actively breaking the rules, please create a ticket.");
            builder.AddField(":star2: Current Rep", $"```diff\n+{repUser.GetCurrentRep()} ({repUser.GetCurrentRep(lib.Data.RepType.Positive)}:{repUser.GetCurrentRep(lib.Data.RepType.Negative)})```", true);
            builder.AddField(":pencil2: Reason", $"```{reason}```", true);

            builder.WithFooter($"given by {giveUser.GetUserInfo(Context.Guild).UsernameFull} | RepID: pending", Context.User.GetAvatarUrl());
            Discord.Rest.RestUserMessage message = (Discord.Rest.RestUserMessage)await ReplyAsync(embed: builder.Build());


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
                                await GiveRep(giveUser, repUser, false, reason);
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

        public async Task LogRep(Reputation rep)
        {
            // send to admin channel
        }

    }
}