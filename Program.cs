using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Winston_Build3.Modules.Music.Youtube;
using Winston_Build3.Modules.Music;

namespace Winston_Build3
{
    public class Program
    {

        private DiscordSocketClient client;
        private CommandHandler handler;
        private readonly string token = "";

        /**
         */
        public static void Main(string[] args) =>
            new Program().MainAsync().GetAwaiter().GetResult();

        /**
         */
        private async Task MainAsync()
        {

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
    

            var serviceProvider = ConfigureServices();
            handler = new CommandHandler(serviceProvider);
            await handler.ConfigureAsync();


            await Task.Delay(-1);
        }

        /**
         */
        private IServiceProvider ConfigureServices()
        {

            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new EventHandler(client))
                .AddSingleton(new AdminService())
                .AddSingleton(new YoutubeSearchService())
                .AddSingleton(new DiscordMusicPlayerService())
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false }));


            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);

            return provider;
        }

        /**
         */
        private Task Log(LogMessage msg)
        {
            var x = Console.ForegroundColor;

            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    x = ConsoleColor.Red;
                    break;
                case LogSeverity.Error:
                    x = ConsoleColor.Magenta;
                    break;
                case LogSeverity.Warning:
                    x = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Verbose:
                    x = ConsoleColor.Cyan;
                    break;
                case LogSeverity.Debug:
                    x = ConsoleColor.White;
                    break;

            }

            Console.ForegroundColor = x;
            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source} : {msg.Message}");


            return Task.CompletedTask;
        }
    }
}
