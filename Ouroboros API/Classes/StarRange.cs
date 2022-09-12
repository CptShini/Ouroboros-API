namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// !!!CUSTOM MADE!!! A class containing information a range of star difficulties.
    /// </summary>
    public class StarRange
    {

        /// <summary>
        /// The corrosponding name to the range of stars.
        /// </summary>
        public string name;

        /// <summary>
        /// The minimum star difficulty.
        /// </summary>
        public int minStars;

        /// <summary>
        /// The maximum star difficulty.
        /// </summary>
        public int maxStars;

        /// <summary>
        /// Create a new star range.
        /// </summary>
        /// <param name="min">The minimum star difficulty.</param>
        /// <param name="max">The maximum star difficulty.</param>
        public StarRange(int min, int max)
        {
            name = min < 11 ? $"{min:00}-{max:00}" : $"{min:00}+";
            minStars = min;
            maxStars = min < 11 ? max : 14;
        }
    }
}