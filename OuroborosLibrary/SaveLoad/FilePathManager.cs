namespace OuroborosLibrary.SaveLoad
{
    public static class FilePathManager
    {
        private static string? _beatSaberPath;
        internal static string? UserDataPath { get; private set; }
        internal static string? MainDataPath { get; private set; }
        internal static string? PlaylistPath { get; private set; }
        internal static string? OuroborosPath { get; private set; }

        /// <summary>
        /// Sets up the appropriate folders and directories.
        /// </summary>
        /// <param name="beatSaberLoc">The path to the players Beat Saber playlist directory; example: "E:\Steam\steamapps\common\Beat Saber\"</param>
        public static void InitializeFilePaths(string beatSaberLoc)
        {
            _beatSaberPath = beatSaberLoc;
            UserDataPath = $@"{_beatSaberPath}UserData\Ouroboros\";
            MainDataPath = $@"{UserDataPath}SaveLoadData\";

            PlaylistPath = $@"{_beatSaberPath}Playlists\";
            OuroborosPath = $@"{PlaylistPath}0uroboros\";
            CreateRequiredFolders();
        }

        /// <summary>
        /// Creates the required folders for the program to work, unless already made.
        /// </summary>
        private static void CreateRequiredFolders()
        {
            Directory.CreateDirectory($@"{MainDataPath}");

            Directory.CreateDirectory($@"{OuroborosPath}#\");
            Directory.CreateDirectory($@"{OuroborosPath}Sniping\");
            Directory.CreateDirectory($@"{OuroborosPath}Øuroboros\");

            Directory.CreateDirectory($@"{PlaylistPath}YourOtherPlaylists\");
        }
    }
}