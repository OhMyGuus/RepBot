using Discord.Commands;
using Microsoft.Extensions.Logging;
using RepBot.lib;
using RepBot.lib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepBot.Modules
{
    public class BotModuleBase : ModuleBase<SocketCommandContext>
    {
        protected readonly ILogger<ReputationModule> _logger;
        protected DiscordServer server => DiscordServerStore.getInstance().GetServer(Context.Guild.Id);

        protected RepUser GetRepUser(string userId = null)
        {
            var userMention = Context.Message.MentionedUsers.FirstOrDefault();
            if (userMention != null)
            {
                return server.GetRepUser(Context.Guild, userMention.Id);
            }
            else
            {
                if (userId != "-1" && ulong.TryParse(userId, out ulong parsedId) && (Context.Guild.GetUser(parsedId) != null || server.GetRepUserOrNull(parsedId) != null))
                {
                    return server.GetRepUser(Context.Guild, parsedId);
                }
            }
            return null;
        }

        public async Task Log(string log)
        {
            try
            {
                if (server.Settings.LogChannelId != 0)
                {
                
                    await Context.Guild.GetTextChannel(server.Settings.LogChannelId)?.SendMessageAsync("\r\n **:notepad_spiral: RepLogs | **" + log);
                }
            }
            catch
            {
                // ignore
            }
        }

        public BotModuleBase(ILogger<ReputationModule> logger)
          => _logger = logger;

    }
}
