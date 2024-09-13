using OuroborosLibrary.ScoreSaberClasses;

namespace OuroborosLibrary.Queries
{
    internal class Collection<T> // I absolutely fucking hate this...
    {
        internal T[] _array;
        internal Metadata _metadata;

        /// <summary>
        /// Creates a generic version of the ScoreSaber API's collections.
        /// </summary>
        /// <param name="array">The array of items to be held in this collection.</param>
        /// <param name="metadata">The metadata of the item array.</param>
        internal Collection(T[] array, Metadata metadata)
        {
            _array = array;
            _metadata = metadata;
        }
    }
}