namespace OuroborosLibrary.ScoreSaberClasses
{
    /// <summary>
    /// A Class containing all information relevant to a leaderboard, aka. a map.
    /// </summary>
    public class LeaderboardInfo : IEquatable<LeaderboardInfo>
    {
        #region ScoreSaber Stuff

        /// <summary>
        /// The ID of the map; ID varies depending on selected difficulty, hash does not!
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// The hash of the song; ID varies depending on selected difficulty, hash does not!
        /// </summary>
        public string songHash { get; set; }

        /// <summary>
        /// The name of the song.
        /// </summary>
        public string songName { get; set; }

        /// <summary>
        /// Used to point out covers and remixes.
        /// </summary>
        public string songSubName { get; set; }

        /// <summary>
        /// The artist who made the song.
        /// </summary>
        public string songAuthorName { get; set; }

        /// <summary>
        /// The mapper who made the map.
        /// </summary>
        public string levelAuthorName { get; set; }

        /// <summary>
        /// The difficulty information of the map.
        /// </summary>
        public Difficulty difficulty { get; set; }

        /// <summary>
        /// The max possible score on the map, without multipliers; can be incorrect on certain older maps.
        /// </summary>
        public int maxScore { get; set; }

        /// <summary>
        /// The time at which the map was created. (Submitted to BeatSaver)
        /// </summary>
        public DateTime createdDate { get; set; }

        /// <summary>
        /// The time at which the map got ranked.
        /// </summary>
        public DateTime rankedDate { get; set; }

        /// <summary>
        /// The time at which the map got qualified for ranked.
        /// </summary>
        public DateTime qualifiedDate { get; set; }

        /// <summary>
        /// The time at which the map got set as loved.
        /// </summary>
        public DateTime lovedDate { get; set; }

        /// <summary>
        /// Whether or not the map is ranked.
        /// </summary>
        public bool ranked { get; set; }

        /// <summary>
        /// Whether or not the map has qualified to be ranked.
        /// </summary>
        public bool qualified { get; set; }

        /// <summary>
        /// Whether or not the map is loved.
        /// </summary>
        public bool loved { get; set; }

        /// <summary>
        /// What is the max possible PP from the map. DISCLAIMER: always seems to be -1?
        /// </summary>
        public float maxPP { get; set; }

        /// <summary>
        /// The star difficulty of the map.
        /// </summary>
        public float stars { get; set; }

        /// <summary>
        /// Whether or not the map allows positive modifiers.
        /// </summary>
        public bool positiveModifiers { get; set; }

        /// <summary>
        /// The total number of plays on the map.
        /// </summary>
        public int plays { get; set; }

        /// <summary>
        /// The number of daily plays on the map.
        /// </summary>
        public int dailyPlays { get; set; }

        /// <summary>
        /// The link to the maps cover image.
        /// </summary>
        public string coverImage { get; set; }

        /// <summary>
        /// The player's score on the map; only relevant in certain cases.
        /// </summary>
        public Score playerScore { get; set; }

        /// <summary>
        /// The difficulties available on the map; only relevant in certain cases.
        /// </summary>
        public Difficulty[] difficulties { get; set; }

        #endregion

        #region Custom Stuff

        public string DifficultyName { get; private set; }
        public string BeatmapName { get; private set; }
        public string MapPlaylistString => GetSongString();

        internal void UpdateMapInfo()
        {
            DifficultyName = ResolveDifficultyName(difficulty.difficulty);
            BeatmapName = $"{songName} ({DifficultyName})";
        }

        private string GetSongString()
        {
            string result =
                "    {\n" +
               $"      \"hash\": \"{songHash}\",\n" +
                "      \"difficulties\": [\n" +
                "        {\n" +
                "          \"characteristic\": \"Standard\",\n" +
               $"          \"name\": \"{DifficultyName}\"\n" +
                "        }\n" +
                "      ]\n" +
                "    },\n";

            return result;
        }

        /// <summary>
        /// Resolves what the name of a given difficulty is. (1 = Easy, 3 = Normal, 5 = Hard, 7 = Expert, 9 = Expert+)
        /// </summary>
        /// <param name="diff">The number corrosponding to a given difficulty. (1 = Easy, 3 = Normal, 5 = Hard, 7 = Expert, 9 = Expert+)</param>
        /// <returns>The corrosponding difficulty name.</returns>
        private static string ResolveDifficultyName(int diff)
        {
            return diff switch
            {
                1 => "Easy",
                3 => "Normal",
                5 => "Hard",
                7 => "Expert",
                9 => "ExpertPlus",
                _ => "Broken",
            };
        }

        public bool Equals(LeaderboardInfo? other) => id == other?.id;

        public override int GetHashCode() => id.GetHashCode();

        public override bool Equals(object? obj) => Equals(obj as LeaderboardInfo);

        #endregion
    }
}