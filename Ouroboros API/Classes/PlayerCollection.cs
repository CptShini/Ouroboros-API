namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing a collection of players and information regarding those.
    /// </summary>
    public class PlayerCollection
    {

        /// <summary>
        /// The collection of players.
        /// </summary>
        public Player[] players { get; set; }

        /// <summary>
        /// The metadata regarding those players.
        /// </summary>
        public Metadata metadata { get; set; }

    }
    
}
