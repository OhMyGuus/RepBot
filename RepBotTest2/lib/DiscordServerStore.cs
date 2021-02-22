using Newtonsoft.Json;
using RepBot.lib.Data;
using RepBot.lib.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RepBot.lib
{
    public class DiscordServerStore
    {
        private static DiscordServerStore Instance = null;

        public Dictionary<ulong, DiscordServer> DiscordServers { get; set; } = new Dictionary<ulong, DiscordServer>();
        const string DATAFILENAME = "data.json";
        private DiscordServerStore()
        {
            Load();
        }

        public DiscordServer GetServer(ulong id)
        {
            if (DiscordServers.ContainsKey(id))
            {
                return DiscordServers[id];
            }
            else
            {
                throw new ServerNotConfiguredException();
            }
        }

        public void ConfigureServer(ulong id, DiscordServerSettings settings)
        {
            if (DiscordServers.ContainsKey(id))
            {
                DiscordServers[id].Settings = settings;
            }
            else
            {
                DiscordServers.Add(id, new DiscordServer(id, settings));
            }
            Save();
        }

        #region loading & sigelton

        public string ToJson()
        {
            return JsonConvert.SerializeObject(DiscordServers);
        }
        public void Save()
        {
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + DATAFILENAME, ToJson());
        }

        private void Load()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + DATAFILENAME))
            {
                string data = File.ReadAllText(DATAFILENAME);
                DiscordServers = JsonConvert.DeserializeObject<Dictionary<ulong, DiscordServer>>(data);
            }

        }
        public static DiscordServerStore getInstance()
        {
            if (Instance == null)
            {
                Instance = new DiscordServerStore();
            }
            return Instance;
        }
        #endregion

    }
}
