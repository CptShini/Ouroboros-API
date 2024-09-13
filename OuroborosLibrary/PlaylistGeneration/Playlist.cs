using OuroborosLibrary.ScoreSaberClasses;

namespace OuroborosLibrary.PlaylistGeneration
{
    public class Playlist
    {
        public readonly string Title;
        private readonly string _author = "Ouroboros";
        private readonly IEnumerable<LeaderboardInfo> _maps;

        private readonly bool _empty;
        private readonly bool _placeholder;
        public readonly bool Invalid;

        /// <summary>
        /// Creates an empty playlist with the given title.
        /// </summary>
        /// <param name="title">The title of the playlist displayed in game.</param>
        internal Playlist(string title)
        {
            Title = title;
            _maps = Array.Empty<LeaderboardInfo>();

            _empty = !_maps.Any();
            _placeholder = Title.Contains("placeholder");
            Invalid = _empty && !_placeholder;
        }

        /// <summary>
        /// Creates a playlist from the given maps and title.
        /// </summary>
        /// <param name="title">The title of the playlist displayed in game.</param>
        /// <param name="maps">The collection of maps to be converted into a playlist.</param>
        public Playlist(string title, IEnumerable<LeaderboardInfo> maps) : this(title) => _maps = maps;

        /// <summary>
        /// Creates a playlist from the given maps and title.
        /// </summary>
        /// <param name="title">The title of the playlist displayed in game.</param>
        /// <param name="maps">The collection of playerscores to be converted into a playlist.</param>
        public Playlist(string title, IEnumerable<PlayerScore> maps) : this(title) => _maps = maps.Select(ps => ps.leaderboard);

        /// <summary>
        /// Gets the string for a given playlist; this is the exact contents of a .bplist file.
        /// </summary>
        /// <returns>A string representing a playlist.</returns>
        public string GetPlaylistString()
        {
            string result =
                "{\n" +
               $"  \"playlistTitle\": \"{Title}\",\n" +
               $"  \"playlistAuthor\": \"{_author}\",\n" +
                "  \"image\": \"\",\n" +
                "  \"songs\": [\n" +
                        GetSongStrings() +
                "  ]\n" +
                "}";

            return result;
        }

        /// <summary>
        /// Gets the song string for all maps in playlist.
        /// </summary>
        /// <returns>A string containing song strings for all maps in playlist.</returns>
        private string GetSongStrings()
        {
            string result = "";

            foreach (LeaderboardInfo map in _maps)
            {
                result += map.MapPlaylistString;
            }

            return result.TrimEnd(',');
        }
    }
}