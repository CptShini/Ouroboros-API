using static OuroborosLibrary.SaveLoad.FileSaveLoadManager;
using static OuroborosLibrary.SaveLoad.FilePathManager;
using static OuroborosLibrary.SaveLoad.Base64FileNameConverter;

namespace OuroborosLibrary.PlaylistGeneration
{
    public static class PlaylistGenerator
    {
        /// <summary>
        /// Creates a playlist readable by Beat Saber (.bplist) from a playlist made with this program.
        /// </summary>
        /// <param name="path">The local path inside the Beat Saber playlist directory. Example: Beat Saber\Playlists\"Sniping\"[some playlist.bplist].</param>
        /// <returns>True, if playlist generation was successful. Otherwise false.</returns>
        public static bool GenerateBPList(this Playlist playlist, string path)
        {
            if (playlist.Invalid) return false;

            string contents = playlist.GetPlaylistString();
            string salt = Base64Encode(DateTime.Now.Millisecond.ToString()); // To make sure Beat Saber doesn't simply show an old playlist by the same name.
            string finalTitle = CleanFileName($"{playlist.Title} ({salt})");
            string fullPath = $"{OuroborosPath}{path}{finalTitle}.bplist";

            SaveFile(fullPath, contents);
            return true;
        }

        /// <summary>
        /// Removes any illegal characters from the filename.
        /// </summary>
        /// <param name="fileName">The file name to be cleaned.</param>
        /// <returns>The cleaned file name.</returns>
        private static string CleanFileName(string fileName)
        {
            fileName = fileName.Replace("ツ", ""); // There isn't a good reason other than the program hates me if I don't do this.
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty)); // I barely even know how this works, don't remember writing it.
        }
    }
}