using System.Collections.Generic;
using System.Diagnostics;
using Discord.Audio;
using Discord;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Winston_Build3.Modules.Music
{
    public class DiscordMusicPlayerService
    {
        private LinkedList<string> songQueue;

        private IAudioClient audioClient;
        private Stream output;
        private AudioOutStream stream;

        public AudioClientState AudioState;
        public AudioRepeatState RepeatState;
        private ConnectionState PlayerConnectionState;

        public DiscordMusicPlayerService()
        {
            songQueue = new LinkedList<string>();
            AudioState = AudioClientState.Stopped;
            RepeatState = AudioRepeatState.NotRepeat;
            PlayerConnectionState = ConnectionState.Disconnected;
        }
        
        /**
         * <summary>
         * Checks whtether or not queue has 0 items or not
         * </summary>
         * 
         * <returns>
         * bool, based on if the queue is empty or not.
         * </returns>
         */
        public bool QueueIsEmpty() {
            return songQueue.Count == 0;
        }

        /**
         * <summary>
         * Creates a stream process
         * </summary>
         * 
         * <param name="url">
         * Url to be used in youtube-dl
         * </param>
         * 
         * <returns>
         * cmd.exe process that runs youtube-dl and ffmpeg
         * </returns>
         */
        private Process CreateStream(string url)
        {
            Process currentSong = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C youtube-dl.exe -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            currentSong.Start();
            return currentSong;
        }

        /**
         * <summary>
         * Adds url to queue
         * </summary>
         * 
         * <param name="url">
         * Url to be added to queue
         * </param>
         */
        public void Queue(string url)
        {
            songQueue.AddFirst(url);
        }
          
        /**
         * <summary>
         * Creates Streams and sends audio to voice channel.
         * </summary>
         * 
         * <param name="url">
         * url to be used in the stream. Should be first element from songQueue
         * </param>
         */
        public async Task SendAudioAsync(string url)
        {
            //Check that Audio Client is Connected or Connecting.
            if (PlayerConnectionState == ConnectionState.Disconnected || PlayerConnectionState == ConnectionState.Disconnecting)
            {
                Console.WriteLine("This shouldn't be happening");
                Console.WriteLine("Clearing Queue and ending streams.");
                songQueue.Clear();
                output.Dispose();
                stream.Dispose();
                return;
            }

            try
            {
                output = CreateStream(url).StandardOutput.BaseStream;
                stream = audioClient.CreatePCMStream(AudioApplication.Music, 128 * 1024);

                AudioState = AudioClientState.Playing;
                await output.CopyToAsync(stream);

                await stream.FlushAsync().ConfigureAwait(false);
                AudioState = AudioClientState.Stopping;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"Exceptions thrown in SendAudioAsync");
                //print exceptions
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }

        }

        /**
         * <summary>
         * Remove player from void channel
         * </summary>
         */
        public async Task LeaveAudioAsync()
        {

            Console.WriteLine("Leaving Channel");

            PlayerConnectionState = ConnectionState.Disconnecting;
            await audioClient.StopAsync();
            AudioState = AudioClientState.Stopped;
            PlayerConnectionState = ConnectionState.Disconnected;

            Console.WriteLine("Left");
 
        }

        /**
         * <summary>
         * Gets voice channel user is in and connects audioClient to voice channel
         * </summary>
         * 
         * <param name="voiceChannel">
         * voice channel user is in.
         * IVoiceChannel => (Context.User as IVoiceState).VoiceChannel
         * </param>
         */
        public async Task JoinAudioAsync(IVoiceChannel voiceChannel)
        {
            PlayerConnectionState = ConnectionState.Connecting;
            audioClient = await voiceChannel.ConnectAsync();
            PlayerConnectionState = ConnectionState.Connected;
        }

        /**
         * <summary>
         * Disconnects player from audio channel and clears queue
         * </summary>
         * 
         * <remarks>
         * Sets AudioClientState to Stopped
         * </remarks>
         */
        public async Task DisconnectPlayer()
        {
            try
            {
                Console.WriteLine("Disconnecting...");
                PlayerConnectionState = ConnectionState.Disconnecting;
                await audioClient.StopAsync();
                Console.WriteLine("Disconnected.");
                PlayerConnectionState = ConnectionState.Disconnecting;

                AudioState = AudioClientState.Stopped;

                ClearQueue();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Exception thrown in DisconnectPlayer");
                //print exceptions
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }

            }
        }

        /**
         * <summary>
         * Force disconnects player from audio channel, clear queue, end streams.
         * </summary>
         */
        public async Task ForceDisconnect()
        {
            AudioState = AudioClientState.Stopping;
            PlayerConnectionState = ConnectionState.Disconnecting;

            await audioClient.StopAsync();

            AudioState = AudioClientState.Stopped;
            PlayerConnectionState = ConnectionState.Disconnected;

            ClearQueue();
            //output.Clear();
            //stream.Clear();

        }

        /**
         * <summary>
         * Gets the queue
         * </summary>                                       
         * 
         * <returns>
         * the songQueue with urls
         * </returns>
         */
        public LinkedList<string> GetQueue()
        {
            return songQueue;
        }

        /**
         * <summary>
         * Gets the amount of elements in the queue remaining
         * </summary>
         * 
         * <returns>
         * An int representing the amount of elements remaining
         * </returns>
         */
        public int QueueSize()
        {
            return songQueue.Count;
        }

        /**
         * <summary>
         * Removes and gets first song from queue
         * </summary>
         * 
         * <returns>
         * First element of songQueue
         * </returns>
         */
        public string Dequeue()
        {
            string temp = songQueue.First.Value;
            songQueue.RemoveFirst();
            return temp;
        }

        /**
         * <summary>
         * Peeks the first song from queue without removing
         * </summary>
         */
        public string Peek()
        {
            return songQueue.First.Value;
        }

        // Unimplemented
        /**
         * <summary>
         * Skips current song
         * </summary>
         * 
         * <remarks>
         * Not Implemented
         * </remarks>
         */
        public async Task SkipMusic()
        {
            output.Dispose();
            stream.Dispose();
        }

        // Depreciated
        /**
         * <summary>
         * Converts the current queue to an array.
         * </summary>
         * 
         * <returns>
         * array with elements of the queue inside
         * </returns>
         */
        public string[] QueueToArray()
        {
            string[] songArray = new string[songQueue.Count];
            songQueue.CopyTo(songArray, 0);
            return songArray;
        }
        
        /**
         * <summary>
         * Clears song queue.
         * </summary>
         */
        public void ClearQueue()
        {
            songQueue.Clear();
        }

        /**
         * <summary>
         * Sets the audio player's RepeatState to the one specified
         * </summary>
         * 
         * <param name="state">
         * an AudioRepeatState that specifies the state to set the player to:
         *  - Repeat
         *  - NotRepeat
         * </param>
         */
        public void Repeat(AudioRepeatState state)
        {
            RepeatState = state;
        }

        /**
         * <summary>
         * States of the audio player:
         *  - Playing
         *  - Stopping
         *  - Stopped
         * </summary>
         */
        public enum AudioClientState : byte
        {
            Playing = 0,
            Stopping = 1,
            Stopped = 2,
        }

        /**
         * <summary>
         * Repeat states of audio player:
         *  - Repeat
         *  - NotRepeat
         * </summary>
         */
        public enum AudioRepeatState : byte
        {
            Repeat = 1,
            NotRepeat = 2,
        }
    }

}
