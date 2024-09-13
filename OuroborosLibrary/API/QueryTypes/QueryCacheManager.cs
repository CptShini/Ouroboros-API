using static OuroborosLibrary.SaveLoad.Base64FileNameConverter;
using static OuroborosLibrary.SaveLoad.FilePathManager;

namespace OuroborosLibrary.API.QueryTypes
{
    internal abstract class QueryCacheManager
    {
        protected readonly string TargetURL;
        protected readonly string FilePath;
        protected readonly bool TypeShouldBeCached;

        protected QueryCacheManager(string targetURL, bool cachedType)
        {
            TargetURL = targetURL; 
            FilePath = $"{MainDataPath}{Base64Encode(targetURL)}.txt";
            TypeShouldBeCached = cachedType;
        }

        #region Save

        internal void CacheResponse(string response)
        {
            if (TypeShouldBeCached) Cache(response);
        }
        protected abstract void Cache(string queryResponse);

        #endregion

        #region Load

        internal bool TryLoadLocalCache(out string cachedData)
        {
            cachedData = "";

            if (!TypeShouldBeCached) return false;

            bool cacheFound = LoadCache(out cachedData);
            if (!cacheFound) return false;

            bool cacheUpToDate = IsCacheUpToDate(cachedData);
            if (!cacheUpToDate) DeleteCache();

            return cacheUpToDate;
        }

        protected abstract bool LoadCache(out string cache);
        protected abstract bool IsCacheUpToDate(string loadedCache);
        protected abstract void DeleteCache();

        #endregion
    }

    internal static class QueryCacheManagerFactory
    {
        internal static QueryCacheManager CreateCacheManager(string targetUrl)
        {
            APIDataType type = ResolveTargetURLDataType(targetUrl);
            bool saveable = SaveableAPIDataType(type);
            return type switch
            {
                _ => new TemporaryCacheManager(targetUrl, saveable)
            };
        }

        private static bool SaveableAPIDataType(APIDataType type)
        {
            return type switch
            {
                APIDataType.Leaderboards => true,
                APIDataType.LeaderboardScores => true,
                APIDataType.Players => true,
                APIDataType.PlayerScores => true,

                APIDataType.LeaderboardInfo => false,
                APIDataType.PlayerCount => false,
                APIDataType.PlayerInfo => false,

                _ => false
            };
        }

        private static APIDataType ResolveTargetURLDataType(string targetUrl)
        {
            string[] queryChunks = targetUrl.Split(new[] { '/', '?' }, 4); // Splits the input, example: "player/76561198074878770/basic" => "player", "76561198074878770", "basic".

            string dataType = queryChunks[0];
            return dataType switch
            {
                "leaderboards" => APIDataType.Leaderboards,
                "leaderboard" => queryChunks[3] is "info" ? APIDataType.LeaderboardInfo : APIDataType.LeaderboardScores,
                "players" => queryChunks[1] is "count" ? APIDataType.PlayerCount : APIDataType.Players,
                "player" => queryChunks[2] is "basic" or "full" ? APIDataType.PlayerInfo : APIDataType.PlayerScores,
                _ => APIDataType.Error,
            };
        }
    }

    public enum APIDataType { AllSaveData, Leaderboards, LeaderboardInfo, LeaderboardScores, Players, PlayerCount, PlayerInfo, PlayerScores, Error };
}