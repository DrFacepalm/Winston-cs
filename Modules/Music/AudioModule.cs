using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Winston_Build3.Modules.Music.Youtube;
using Microsoft.Extensions.DependencyInjection;

namespace Winston_Build3.Modules.Music
{
    [DontAutoLoad]
    public class AudioModule : ModuleBase
    {
        private DiscordMusicPlayerService musicPlayer;
        private YoutubeSearchService youtubeSearch;
        private IServiceProvider map;

        public AudioModule(IServiceProvider provider)
        {
            map = provider;
            musicPlayer = map.GetService<DiscordMusicPlayerService>();
            youtubeSearch = map.GetService<YoutubeSearchService>();
            
        }

        //--------------------------------------------Testing And Development--------------------------------------------//

        /**
         * <summary>
         * Method called by user, searches for music via keyword or url and plays the music through an AudioClient
         * if user is in a voice channel.
         * </summary>
         * 
         * <remarks>
         * Player plays the first video returned from the search
         * </remarks>
         * 
         * <param name="input">
         * Keywords or url input by user.
         * </param>
         */
        [Command("Play", RunMode = RunMode.Async)]
        [Summary("Requests, Queue and play music based on url or keywords")]
        public async Task Play([Remainder]string input)
        {
            //If user is not in a voice channel, return
            if ((Context.User as IGuildUser).VoiceChannel == null)
            {
                await Context.Channel.SendMessageAsync("You need to be in a voice channel to use this command!");
                return;
            }

            //Search for video based on input
            YoutubeSnippetCollection vidInfo = await youtubeSearch.SearchAllAsync(input);

            musicPlayer.Queue(vidInfo.Url);
            await Context.Channel.SendMessageAsync($"{vidInfo.Title} was added to queue.");

            //if player is not stopped, i.e. if player is playing, return
            if (musicPlayer.AudioState != DiscordMusicPlayerService.AudioClientState.Stopped)
            {
                Console.WriteLine($"Player is playing a song, adding {vidInfo.Title} to queue and returning");
                return;
            }
            //otherwise, connect to voice channel
            else
            {
                var voiceChannel = (Context.User as IVoiceState).VoiceChannel;
                Console.WriteLine($"Joining {voiceChannel.Name}");
                await musicPlayer.JoinAudioAsync(voiceChannel);

                while (!musicPlayer.QueueIsEmpty())
                {
                    Console.WriteLine("peeking first item in queue.");
                    string urlTemp = musicPlayer.Peek();

                    Console.WriteLine("sending audio");
                    await musicPlayer.SendAudioAsync(urlTemp);


                    Console.WriteLine($"{musicPlayer.RepeatState.ToString()}");
                    //if repeat is set, add the element being played to queue.
                    if (musicPlayer.RepeatState == DiscordMusicPlayerService.AudioRepeatState.Repeat)
                    {
                        Console.WriteLine("RepeatState = repeat, continuing");
                        continue;
                    }

                    Console.WriteLine("removing item from queue");
                    musicPlayer.Dequeue();

                }

                await musicPlayer.LeaveAudioAsync();

            }
        }

        /**
         * <summary>
         * Disconnects bot from voice channel
         * </summary>
         */
        [Command("Disconnect", RunMode = RunMode.Async)]
        [Alias("dc")]
        [Summary("Disconnects bot from voice channel and clears music queue")]
        public async Task Disconnect()
        {
            await musicPlayer.DisconnectPlayer();
        }

        /**
         * <summary>
         * Negates the isRepeat variable and sets AudioRepeatState to the corresponding state
         * </summary>
         */
        [Command("Repeat", RunMode = RunMode.Async)]
        [Summary("Sets music player's repeat status")]
        public async Task Repeat()
        {

            if (musicPlayer.RepeatState == DiscordMusicPlayerService.AudioRepeatState.NotRepeat)
            {
                musicPlayer.Repeat(DiscordMusicPlayerService.AudioRepeatState.Repeat);
                await Context.Channel.SendMessageAsync("Repeating");
            }
            else
            {
                musicPlayer.Repeat(DiscordMusicPlayerService.AudioRepeatState.NotRepeat);
                await Context.Channel.SendMessageAsync("Not Repeating");
            }
           
        }

        /**
         * <summary>
         * Skips current song
         * </summary>
         */
        [Command("Skip", RunMode = RunMode.Async)]
        [Summary("Skips current song")]
        public async Task Skip()
        {
            await musicPlayer.SkipMusic();
        }

        [Command("Queue")]
        [Summary("Shows first 10 elements of the queue.")]
        public async Task QueueFive()
        {
            string s = "";

            int total;

            if (musicPlayer.QueueSize() < 10)
            {
                total = musicPlayer.QueueSize();
            }
            else
            {
                total = 10;
            }

            for (int i = 0; i < total; i++)
            {
                s += $"{i + 1}. {youtubeSearch.SearchAllAsync(musicPlayer.QueueToArray()[i]).Result.Title}\n";
            }
            
            await Context.Channel.SendMessageAsync(s);
            if (musicPlayer.QueueSize() > 10)
            {
                await Context.Channel.SendMessageAsync($"There are more than 10 items in queue, use \"QueueAll\" to view everything.");
            }
        }

        [Command("QueueAll")]
        [Summary("Shows everything in the queue.")]
        public async Task QueueAll()
        {
            string s = "";
            int i = 1;
            foreach (string title in musicPlayer.QueueToArray())
            {
                s += $"{i}. {youtubeSearch.SearchAllAsync(musicPlayer.QueueToArray()[i]).Result.Title}";
            }
            await Context.Channel.SendMessageAsync(s);
        }

        [Command("Reset")]
        [Alias("Restart")]
        [Summary("Disconnects player, clears queue.")]
        public async Task Reset()
        {
            musicPlayer.ClearQueue();
            musicPlayer.AudioState = DiscordMusicPlayerService.AudioClientState.Stopped;
            await musicPlayer.DisconnectPlayer();
            await Context.Channel.SendMessageAsync("reset");
        }
    }
}
