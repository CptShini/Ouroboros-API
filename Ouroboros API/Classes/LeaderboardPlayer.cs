namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing the basic information regarding a player on a leaderboard.
    /// </summary>
    public class LeaderboardPlayer
    {

        /// <summary>
        /// The ID of the player.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The link to the player's profile picture.
        /// </summary>
        public string profilePicture { get; set; }

        /// <summary>
        /// The country of the player.
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// The permissions of the player.
        /// </summary>
        public int permissions { get; set; }

        /// <summary>
        /// The role of the player.
        /// </summary>
        public string role { get; set; }

    }
}
