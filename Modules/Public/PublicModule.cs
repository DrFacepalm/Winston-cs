using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Diagnostics;

namespace Winston_Build3.Modules.Public
{
    public class PublicModule : ModuleBase
    {

        [Command("Ping")]
        [Summary("Says Pong")]
        public async Task Ping()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await Context.Channel.SendMessageAsync("Pong!");
            sw.Stop();
            await Context.Channel.SendMessageAsync("Response took: " + sw.ElapsedMilliseconds.ToString() + "ms.");
        }


        [Command("purge")]
        [Alias("delete")]
        [Summary("Deletes a bunch of messages")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Purge(
            [Summary("Optional specific user to delete messages from.")] IUser user = null,
            [Summary("Amount of messages to be deleted")] int amount = 10)
        {

            int index = 0;
            var messagesToDelete = new List<IMessage>();

            var messages = Context.Channel.GetMessagesAsync();
            await messages.ForEachAsync(async m =>
            {
                IEnumerable<IMessage> toDelete = null;

                if (user == null)
                {
                    Console.WriteLine("delete = m");
                    toDelete = m;
                }
                else if (user != null)
                {
                    Console.WriteLine("delete = @user m");
                    toDelete = m.Where(msg => msg.Author.Id == user.Id);
                }

                foreach (var msg in toDelete/*.OrderByDescending(msg => msg.Timestamp)*/)
                {
                    messagesToDelete.Add(msg);
                    index++;

                    if (index > amount || index >= toDelete.ToArray().Length)
                    {
                        await Context.Channel.DeleteMessagesAsync(messagesToDelete);
                        Console.WriteLine("Messages Deleted");
                        Console.WriteLine(PrintMsges(messagesToDelete));
                        return;
                    }
                }
            });
        }

        [Command("purge")]
        [Alias("delete")]
        [Summary("Deletes a bunch of messages")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Purge(
            [Summary("Amount of messages to be deleted")] int amount = 10)
        {
            int index = 0;
            var messagesToDelete = new List<IMessage>();

            var messages = Context.Channel.GetMessagesAsync();
            await messages.ForEachAsync(async m =>
            {
                IEnumerable<IMessage> delete = null;
                Console.WriteLine("delete = m");
                delete = m;

                foreach (var msg in delete/*.OrderByDescending(msg => msg.Timestamp)*/)
                {
                    Console.WriteLine("foreach var msg in delete");
                    messagesToDelete.Add(msg);
                    index++;

                    if (index > amount || index >= delete.ToArray().Length)
                    {
                        await Context.Channel.DeleteMessagesAsync(messagesToDelete);
                        Console.WriteLine("Messages Deleted");
                        Console.WriteLine(PrintMsges(messagesToDelete));
                        return;
                    }
                }
            });
        }

        /**
         * Helper method in purge
         */
        public string PrintMsges(List<IMessage> list)
        {
            string result = "";
            foreach (var msg in list)
            {
                result += msg.ToString() + ", ";
            }
            return result;
        }


        [Command("Nazi")]
        [Summary("nazi")]
        public async Task Nazi()
        {
            string s = (
                ":black_large_square::black_large_square::black_large_square::white_large_square::black_large_square:\n" +
                ":white_large_square::white_large_square::black_large_square::white_large_square::black_large_square:\n" +
                ":black_large_square::black_large_square::black_large_square::black_large_square::black_large_square:\n" +
                ":black_large_square::white_large_square::black_large_square::white_large_square::white_large_square:\n" +
                ":black_large_square::white_large_square::black_large_square::black_large_square::black_large_square:\n");

            await Context.Channel.SendMessageAsync(s);


        }

        [Command("Say")]
        [Summary("Winston dm's the user specified and sends the message.")]
        public async Task Say(IUser user, [Remainder] string message)
        {
            var channel = await user.GetOrCreateDMChannelAsync();
            string author = Context.User.Username;
            await channel.SendMessageAsync(String.Format("{0} says: \"{1}\"", author, message));
        }

        [Command("whois")]
        [Alias("userinfo")]
        [Summary("Gives Information about a user")]
        public async Task WhoIs(IUser user)
        {
            //cast the entered user as the GuildUser
            var gUser = user as IGuildUser;

            //collect data about user
            string username = gUser.Username;
            string nickname = gUser.Nickname;
            if (nickname == null)
                nickname = gUser.Username;
            string guild = gUser.Guild.Name;
            string joinedAt = gUser.JoinedAt.ToString();
            string permissions = gUser.GuildPermissions.ToString();


            string s = string.Format(
                "User:\t\t{0}\n" +
                "Nickname:\t\t{1}\n" +
                "Guild:\t\t{2}\n" +
                "Joined At:\t\t{3}\n" +
                "Permissions:\t\t{4}\n",
                username, nickname, guild, joinedAt, permissions
                );

            await Context.Channel.SendMessageAsync(s);

            IEnumerable<ulong> ids = gUser.RoleIds;
            string idString = "";
            foreach (var x in ids)
            {
                idString += x + ", ";
            }
            await Context.Channel.SendMessageAsync(idString);
        }

        [Command("gay")]
        public async Task Gay()
        {
            await Context.Channel.SendMessageAsync("ben");
        }

        [Command("retard")]
        public async Task Retard()
        {
            await Context.Channel.SendMessageAsync("Adam");
        }
        //--------------------------------------------Testing And Development--------------------------------------------//

        [Command("ChannelInfo")]
        [Summary("Gives Information about the channel")]
        public async Task ChannelInfo()
        {
            var guild = Context.Guild;
            string name = guild.Name;
            string creation = guild.CreatedAt.ToString();
            IEnumerable<IRole> roles = guild.Roles;
            foreach (var role in roles)
            {
                await Context.Channel.SendMessageAsync(role.Name);
            }
        }

        [Command("test", RunMode = RunMode.Async)]
        public async Task Test()
        {
            await Context.Channel.SendMessageAsync("Hello Git");
        }


        //Help Command
    }
}
