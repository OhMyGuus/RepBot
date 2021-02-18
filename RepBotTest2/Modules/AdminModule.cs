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

      

    }
}