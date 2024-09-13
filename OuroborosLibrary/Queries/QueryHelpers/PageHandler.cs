namespace OuroborosLibrary.Queries.QueryHelpers
{
    internal static class PageHandler
    {
        /// <summary>
        /// Gets and conjoins all pages gotten from the pageFunction based on how many items were requested.
        /// </summary>
        /// <typeparam name="T">The type of array to get-conjoin and return.</typeparam>
        /// <param name="desiredAmount">The desired number of elements to get.</param>
        /// <param name="pageCollectionFunction">The function that gets each individual page/collection based of a integer page number.</param>
        /// <param name="startingPage">The page at which the algorithm starts.</param>
        /// <returns>An array of type T containing all of the pages' contents joined together.</returns>
        internal static T[] GetConjoinPages<T>(int desiredAmount, Func<int, Collection<T>> pageCollectionFunction, int startingPage = 2)
        {
            Collection<T> initialCollection = pageCollectionFunction(1);

            int getAmount = ItemCountLimiter(desiredAmount, initialCollection._metadata.total);

            List<T> pages = GetConjoinPages(getAmount, initialCollection._metadata.itemsPerPage, startingPage, (int page) => pageCollectionFunction(page)._array);
            if (startingPage == 2) pages.InsertRange(0, initialCollection._array);

            return pages.Take(getAmount).ToArray();
        }

        /// <summary>
        /// Conjoins all pages gotten from the pageFunction.
        /// </summary>
        /// <typeparam name="T">The type of list to return.</typeparam>
        /// <param name="getAmount">The total amount of items.</param>
        /// <param name="pageLength">The length of each page.</param>
        /// <param name="startingPage">The page at which the algorithm starts.</param>
        /// <param name="pageFunction">The function that actually gets each individual page.</param>
        /// <returns>A list of type T containing all of the pages' contents joined together.</returns>
        private static List<T> GetConjoinPages<T>(int getAmount, int pageLength, int startingPage, Func<int, T[]> pageFunction)
        {
            int fullPageCount = getAmount / pageLength;
            int leftoverItems = getAmount % pageLength;
            bool containsIncompletePage = leftoverItems > 0;

            int totalPageCount = containsIncompletePage ? fullPageCount + 1 : fullPageCount;

            List<T> itemList = new();
            for (int currentPage = startingPage; currentPage <= totalPageCount; currentPage++)
            {
                T[] itemPage = pageFunction(currentPage);

                bool finalPage = currentPage >= totalPageCount;
                bool getLeftoverItems = finalPage && containsIncompletePage;
                int itemsToGetOnPage = getLeftoverItems ? leftoverItems : pageLength;

                for (int currentItem = 0; currentItem < itemsToGetOnPage; currentItem++)
                {
                    itemList.Add(itemPage[currentItem]);
                }
            }

            return itemList;
        }

        /// <summary>
        /// Resolves which number is smallest as to never attempt to request a greater number of items than possible.
        /// </summary>
        /// <param name="desiredAmount">The requested number of items; use -1 to return maxCount.</param>
        /// <param name="maxCount">The maximum possible number of items to request.</param>
        /// <returns>The minimum of desiredAmount and maxCount; returns maxCount if desiredAmount = -1.</returns>
        internal static int ItemCountLimiter(int desiredAmount, int maxCount)
        {
            bool requestAboveMax = desiredAmount > maxCount;
            bool maxRequested = desiredAmount == -1;

            return requestAboveMax || maxRequested ? maxCount : desiredAmount;
        }
    }
}