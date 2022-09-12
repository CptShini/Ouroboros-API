namespace Ouroboros_API.ScoreSaberClasses
{
    /// <summary>
    /// A class containing information regarding a corrosponding collection.
    /// </summary>
    public class Metadata
    {

        /// <summary>
        /// The total number of items in the collection.
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// The current page being displayed.
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// The length of each page.
        /// </summary>
        public int itemsPerPage { get; set; }

    }
    
}
