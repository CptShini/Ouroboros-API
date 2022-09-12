namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing a collection of player scores and information regarding those.
    /// </summary>
    public class PlayerScoreCollection
    {

        /// <summary>
        /// The collection of player scores.
        /// </summary>
        public PlayerScore[] playerScores { get; set; }

        /// <summary>
        /// The metadata regarding those player scores.
        /// </summary>
        public Metadata metadata { get; set; }

        public Player player;

    }
    
}
