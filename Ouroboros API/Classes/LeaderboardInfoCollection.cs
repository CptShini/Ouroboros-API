namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing a collection of leaderboards and information regarding those.
    /// </summary>
    public class LeaderboardInfoCollection
    {

        /// <summary>
        /// The collection of leaderboards.
        /// </summary>
        public LeaderboardInfo[] leaderboards { get; set; }

        /// <summary>
        /// The metadata regarding those leaderboards.
        /// </summary>
        public Metadata metadata { get; set; }

    }
}
