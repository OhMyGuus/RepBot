using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RepBot.lib;

namespace RepBot.Services
{
    public class VoiceChannelHandler : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private bool running = false;
        public VoiceChannelHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config)
        {
            _client = client;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!running)
            {
                running = true;
                await Task.Run(() => VoiceHandlerLoop(cancellationToken));
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            running = false;
        }

        private async Task VoiceHandlerLoop(CancellationToken cancellationToken)
        {
            int saveTime = 0;
            while (!cancellationToken.IsCancellationRequested && running)
            {
                await Task.Delay(60000);
                foreach (var server in DiscordServerStore.getInstance().DiscordServers.Values)
                {
                    var guild = _client.GetGuild(server.DiscordServerID);
                    var voiceChannels = guild.VoiceChannels;//.Where(o => o.Users.Count >= 2);
                    foreach (var channel in voiceChannels)
                    {
                        foreach (var user in channel.Users)
                        {
                            var otherUsers = channel.Users;//.Where(o => o.Id != user.Id);
                            foreach (var otherUser in otherUsers)
                            {
                                try
                                {
                                    var repuser = server.GetRepUser(guild, otherUser.Id);
                                    repuser.AddPlayTime(user, 60);
                                }
                                catch (Exception e) { }
                            }
                        }
                    }
                  
                }
                if (saveTime++ == 10)
                {
                    saveTime = 0;
                    DiscordServerStore.getInstance().Save();
                }
            }

        }
    }
}