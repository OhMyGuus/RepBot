using Discord;

namespace RepBot.lib
{
    public class RepUserInfo
    {
        public string Mention { get; set; }
        public string UsernameFull => $"{Username}#{Discriminator}";
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string NickName { get; set; }
        public string AvatarUrl { get; internal set; }

        internal static RepUserInfo GetInfo(IGuildUser user)
        {
            return new RepUserInfo()
            {
                Mention = user.Mention,
                Username = user.Username,
                Discriminator = user.Discriminator,
                NickName = user.Nickname ?? user.Username,
                AvatarUrl = user.GetAvatarUrl()
            };
        }
    }
}