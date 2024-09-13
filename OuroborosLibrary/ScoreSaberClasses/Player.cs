namespace OuroborosLibrary.ScoreSaberClasses
{
    /// <summary>
    /// A class containing all the information relevant to a player.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The ID of the player.
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The link to the players profile picture.
        /// </summary>
        public string profilePicture { get; set; }

        /// <summary>
        /// The country of the player.
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// The amount of PP the player has.
        /// </summary>
        public float pp { get; set; }

        /// <summary>
        /// The global rank of the player.
        /// </summary>
        public int rank { get; set; }

        /// <summary>
        /// The local country rank of the player.
        /// </summary>
        public int countryRank { get; set; }

        /// <summary>
        /// The role of the player.
        /// </summary>
        public string role { get; set; }

        /// <summary>
        /// The badges of the player.
        /// </summary>
        public Badge[] badges { get; set; }

        /// <summary>
        /// The history of what ranks the player has been over the past 50 days.
        /// </summary>
        public string histories { get; set; }

        /// <summary>
        /// Some statistics regarding the players scores.
        /// </summary>
        public ScoreStats scoreStats { get; set; }

        /// <summary>
        /// The permissions level of the player.
        /// </summary>
        public int permissions { get; set; }

        /// <summary>
        /// Whether or not the player is banned.
        /// </summary>
        public bool banned { get; set; }

        /// <summary>
        /// Whether or not the player is inactive.
        /// </summary>
        public bool inactive { get; set; }

    }
}
