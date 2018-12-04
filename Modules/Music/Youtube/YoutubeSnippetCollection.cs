namespace Winston_Build3.Modules.Music.Youtube
{
    public class YoutubeSnippetCollection
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        private static readonly string urlPrefix = "https://www.youtube.com/watch?v=";

        public YoutubeSnippetCollection(string id, string title)
        {
            this.Id = id;
            this.Title = title;
            this.Url = urlPrefix + Id;
        }

    }
}
