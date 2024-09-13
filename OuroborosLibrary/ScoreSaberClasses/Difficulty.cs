namespace OuroborosLibrary.ScoreSaberClasses
{
    /// <summary>
    /// A class containg all relevant information to a difficulty on a map.
    /// </summary>
    public class Difficulty
    {

        /// <summary>
        /// The id of the map it belongs to.
        /// </summary>
        public long leaderboardId { get; set; }

        /// <summary>
        /// The number corrosponding to a given difficulty. (1 = Easy, 3 = Normal, 5 = Hard, 7 = Expert, 9 = Expert+)
        /// </summary>
        public int difficulty { get; set; }

        /// <summary>
        /// What type of map it is.
        /// </summary>
        public string gameMode { get; set; }

        /// <summary>
        /// A string detailing both what difficulty it is, and what gamemode.
        /// </summary>
        public string difficultyRaw { get; set; }

    }
}
