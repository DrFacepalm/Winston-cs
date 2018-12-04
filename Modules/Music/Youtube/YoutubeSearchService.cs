using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;

namespace Winston_Build3.Modules.Music.Youtube
{
    public class YoutubeSearchService
    {
        private readonly YouTubeService youtubeService;
        private static readonly string searchPrefix = "www.youtube.com/watch";

        public YoutubeSearchService()
        {
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBxFZELIWGn9zUudDsxcAvccKSpUGnh6GI",
                ApplicationName = this.GetType().ToString()
            });
        }

        /**
         * <summary>
         * Searches for the first youtube video that matches input keywords
         * </summary>
         * 
         * <param name="input">
         * Keywords to be searched for
         * </param>
         * 
         * <returns>
         * A Task with result: YoutubeSnippetCollection with attributes pertaining to the video found
         * </returns>
         */
        private async Task<YoutubeSnippetCollection> SearchKeywordAsync(string input)
        {

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = input;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            var searchResult = searchListResponse.Items.First(i => i.Id.Kind == "youtube#video");

            return new YoutubeSnippetCollection(searchResult.Id.VideoId, searchResult.Snippet.Title);

        }

        /**
         * <summary>
         * Searches for the video posessing the input id
         * </summary>
         * 
         * <param name="input">
         * id to be searched for
         * </param>
         * 
         * <returns>
         * A Task with result: YoutubeSnippetCollection with attributes pertaining to the video found
         * </returns>
         */
        private async Task<YoutubeSnippetCollection> SearchVideoIdAsync(string input)
        {

            // Create a request for the YoutubeSearchService to search through videos, and list the "snippet" parameters of each video
            var videoListRequest = youtubeService.Videos.List("snippet");

            // Specify the video's id you're looking for
            videoListRequest.Id = input;

            // Execute the request from earlier, and store it in a variable.
            var videoListResponse = await videoListRequest.ExecuteAsync();

            // Find the result from the videoListResponse that has the type of "youtube#video"
            var searchResult = videoListResponse.Items.FirstOrDefault(element => element.Kind == "youtube#video");
            Console.WriteLine(searchResult.Snippet.Title);

            // Return the video Id and title
            return new YoutubeSnippetCollection(searchResult.Id, searchResult.Snippet.Title);

        }

        /**
         * <summary>
         * Takes either a url or keyword, and returns a snippet collection for the video.
         * </summary>
         * 
         * <param name="input">
         * url or keyword to be searched for
         * </param>
         * 
         * <returns>
         * A Task with result: YoutubeSnippetCollection with attributes pertaining to the video found
         * </returns>
         */
        public async Task<YoutubeSnippetCollection> SearchAllAsync(string input)
        {
            //if input is a url
            if (input.Contains(searchPrefix))
            {
                string videoId = input.Substring(input.Length - 11); //11 is typical video id length
                Console.WriteLine(videoId);
                return await SearchVideoIdAsync(videoId);
            }
            //if input is a keyword
            else
            {
                return await SearchKeywordAsync(input);
            }
        }
    }
}
