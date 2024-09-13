namespace OuroborosLibrary.PlaylistGeneration
{
    internal static class PlaylistPlaceholderGenerator
    {
        /// <summary>
        /// Generates the placeholder playlists required for uniformity.
        /// </summary>
        /// <param name="currentPlaylistCount">The number of already existing playlists.</param>
        /// <param name="title">The title of the placeholder playlists.</param>
        /// <param name="path">The local path inside the Beat Saber playlist directory. Example: Beat Saber\Playlists\"Sniping\"[some playlist.bplist].</param>
        internal static void GeneratePlaceholderPlaylists(int currentPlaylistCount, string title, string path)
        {
            for (int i = 0; i < GetPlaceholderCountForUniformity(currentPlaylistCount); i++)
            {
                GenerateEmptyPlaylist($"{title} placeholder {i}", path);
            }
        }

        /// <summary>
        /// Gets the number of placeholder playlists to generate.
        /// </summary>
        /// <param name="playlistCount">The number of already existing playlists.</param>
        /// <returns>The number of placeholder playlists required to fill out the current line of playlists.</returns>
        private static int GetPlaceholderCountForUniformity(int playlistCount)
        {
            int extraPlaylists = playlistCount % 5;
            bool nonUniformPlaylistCount = extraPlaylists > 0;
            int uniformPlaceholderCount = 5 - extraPlaylists;

            return nonUniformPlaylistCount ? uniformPlaceholderCount : 0;
        }

        /// <summary>
        /// Creates an empty playlist.
        /// </summary>
        /// <param name="title">The title of the empty playlist.</param>
        /// <param name="path">The local path inside the Beat Saber playlist directory. Example: Beat Saber\Playlists\"Sniping\"[some playlist.bplist].</param>
        private static void GenerateEmptyPlaylist(string title, string path) => new Playlist(title).GenerateBPList(path);
    }
}