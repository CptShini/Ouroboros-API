namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing all the information regarding a player score.
    /// </summary>
    public class PlayerScore
    {

        /// <summary>
        /// The score the player set.
        /// </summary>
        public Score score { get; set; }

        /// <summary>
        /// The map on which the score was set.
        /// </summary>
        public LeaderboardInfo leaderboard { get; set; }

        /// <summary>
        /// !!!CUSTOM MADE!!! The accuracy of the play; baseScore / maxScore.
        /// </summary>
        public float accuracy;

        public float relativeAccuracy;

        /// <summary>
        /// !!!CUSTOM MADE!!! What % the player is out of all who played it; rank / plays.
        /// </summary>
        public float relativeRank;

    }
}
