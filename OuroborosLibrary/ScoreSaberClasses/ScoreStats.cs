namespace OuroborosLibrary.ScoreSaberClasses
{
    /// <summary>
    /// A class containing statistics regarding a player.
    /// </summary>
    public class ScoreStats
    {

        /// <summary>
        /// The total score of all the players combined plays, ranked or unranked.
        /// </summary>
        public long totalScore { get; set; }

        /// <summary>
        /// The total score of all the players combined ranked plays.
        /// </summary>
        public long totalRankedScore { get; set; }

        /// <summary>
        /// The average accuracy of all the players combined ranked plays.
        /// </summary>
        public float averageRankedAccuracy { get; set; }

        /// <summary>
        /// The total number of plays set by the player, ranked or unranked.
        /// </summary>
        public int totalPlayCount { get; set; }

        /// <summary>
        /// The total number of ranked plays set by the player.
        /// </summary>
        public int rankedPlayCount { get; set; }

        /// <summary>
        /// The number of the player's replays watched by other people.
        /// </summary>
        public int replaysWatched { get; set; }

    }
}
