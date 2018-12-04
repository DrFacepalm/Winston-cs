using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace Winston_Build3
{
    public class EventHandler
    {
        public DiscordSocketClient client;
        public ulong AnnouncementId { get; set; }
        public string DefaultRole { get; set; }

        public EventHandler(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task UserJoinedAsync(SocketGuildUser user)
        {
            Console.WriteLine($"User Joined: {user.Username}");
            //Announce user joined
            try
            {
                var channel = client.GetChannel(AnnouncementId) as SocketTextChannel;
                await channel.SendMessageAsync($"Welcome {user.Mention} to {user.Guild.Name}");
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
            

            //Set new user role
            IRole role;
            //if role is not set or not found, return from thing.
            try
            {
                role = user.Guild.Roles.First(r => r.Name.ToUpper() == DefaultRole.ToUpper()) as IRole;
                await user.AddRoleAsync(role);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine("DefaultRole not set or not found");
                return;
            }

        }

        public async Task UserLeftAsync(SocketGuildUser user)
        {
            Console.WriteLine($"User Left: {user.Username}");
            string name = user.Mention;
            try
            {
                var channel = client.GetChannel(AnnouncementId) as SocketTextChannel;
                await channel.SendMessageAsync($"{name} has left...\nFinally.");
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }

            
            
        }

    }
}
