using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Winston_Build3.Modules.Admin;
using Winston_Build3.Modules.Music.Youtube;
using Winston_Build3.Modules.Music;
using Winston_Build3.Modules.Public;

namespace Winston_Build3
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider map;
        private EventHandler events;
        private AdminService admin;

        public CommandHandler(IServiceProvider provider)
        {
            map = provider;
            client = map.GetService<DiscordSocketClient>();
            commands = map.GetService<CommandService>();
            events = map.GetService<EventHandler>();
            admin = map.GetService<AdminService>();

            client.MessageReceived += HandleCommand;
            client.UserJoined += events.UserJoinedAsync;
            client.UserLeft += events.UserLeftAsync;
                
        }

        /**
         * Adds Modules marked with ModuleBase
         */
        public async Task ConfigureAsync()
        { 
            admin.SetPrefix("!!!");
            admin.SetDefaultRole("Floor Licker");
            admin.SetAnnouncementId(337613536937377793);
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            await commands.AddModuleAsync<AdminModule>();
            await commands.AddModuleAsync<AudioModule>();
            await commands.AddModuleAsync<IServiceProvider>();
        }


        /**
         * Read messages and see whether or not to handle command.
         */
        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasStringPrefix(admin.Prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot)
                return;

            // Create a Command Context
            var context = new CommandContext(client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, map);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
        }
    }
}
