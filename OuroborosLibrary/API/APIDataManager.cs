using OuroborosLibrary.API.QueryTypes;
using static OuroborosLibrary.API.APICommunicator;
using static OuroborosLibrary.API.QueryTypes.QueryCacheManagerFactory;

namespace OuroborosLibrary.API
{
    public static class APIDataManager
    {
        private static readonly Dictionary<string, string> _sessionCache = new();

        /// <summary>
        /// Gets the data from the specified target URL. Checks session cache and local save data before calling API.
        /// </summary>
        /// <param name="query">The request url after the 'https://scoresaber.com/api/'. Example: player/76561198074878770/basic.</param>
        /// <returns>The API's response data, for the given request URL, as a JSON string.</returns>
        public static string GetAPIResponse(string query)
        {
            if (TryLoadSessionCache(query, out string response)) return response; // Checks session cache.

            QueryCacheManager cacheManager = CreateCacheManager(query);
            if (!cacheManager.TryLoadLocalCache(out response)) // Checks local cache.
            {
                response = CallAPI(query); // Gets data from API since session and local cache were empty.
                cacheManager.CacheResponse(response); // Adds data to local cache.
            }

            _sessionCache.Add(query, response); // Adds data to session cache.
            return response;
        }

        private static bool TryLoadSessionCache(string query, out string response) => _sessionCache.TryGetValue(query, out response!);
    }
}