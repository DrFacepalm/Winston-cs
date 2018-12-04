using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord;
using Discord.WebSocket;

namespace Winston_Build3.Modules.Admin
{
    [DontAutoLoad]
    public class AdminModule : ModuleBase
    {
        private AdminService admin;
        private IServiceProvider map;

        public AdminModule(IServiceProvider provider)
        {
            map = provider;
            admin = map.GetService<AdminService>();
        }



        //--------------------------------------------Testing And Development--------------------------------------------//

        // DO NOT USE
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [Command("SetPrefix")]
        public async Task SetPrefix(string input)
        {
            string temp = admin.Prefix;
            admin.SetPrefix(input);
            await Context.Channel.SendMessageAsync($"Prefix changed from \"{temp}\" to \"{input}\"");
        }

        // DO NOT USE
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireBotPermission(Discord.GuildPermission.Administrator)]
        [Command("SetAnnouncementChannel")]
        [Alias("SetChannel")]
        public async Task SetChannel(ITextChannel channel)
        {
            admin.SetAnnouncementId(channel.Id);
            await Context.Channel.SendMessageAsync($"Announcement Channel changed to {channel.Name}");

        }

        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [Command("TestAnnouncement")]
        public async Task TestAnnouncement()
        {
            var channel = Context.Guild.GetTextChannelAsync(admin.AnnouncementId);
            await channel.Result.SendMessageAsync("This is a test announcement.");
        }
    }
}
