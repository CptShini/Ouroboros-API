namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing a collection of scores and information regarding those.
    /// </summary>
    public class ScoreCollection
    {

        /// <summary>
        /// The collection of scores.
        /// </summary>
        public Score[] scores { get; set; }

        /// <summary>
        /// The metadata regarding those scores.
        /// </summary>
        public Metadata metadata { get; set; }

    }
    
}
