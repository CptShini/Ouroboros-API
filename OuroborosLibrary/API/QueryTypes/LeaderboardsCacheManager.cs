using OuroborosLibrary.SaveLoad;
using OuroborosLibrary.ScoreSaberClasses;
using static OuroborosLibrary.SaveLoad.FileSaveLoadManager;

namespace OuroborosLibrary.API.QueryTypes
{
    internal class LeaderboardsCacheManager : QueryCacheManager
    {
        internal LeaderboardsCacheManager(string targetURL) : base(targetURL, true) { }

        protected override void Cache(string queryResponse) => SaveFile(FilePath, queryResponse);
        protected override bool LoadCache(out string cache) => LoadFile(FilePath, out cache);
        protected override bool IsCacheUpToDate(string loadedCache)
        {
            LeaderboardInfoCollection loaded = JSONConverter.DeserializeString<LeaderboardInfoCollection>(loadedCache);

            string APIData = APICommunicator.CallAPI(TargetURL);
            LeaderboardInfoCollection retrieved = JSONConverter.DeserializeString<LeaderboardInfoCollection>(APIData);

            return loaded.metadata.total == retrieved.metadata.total;
        }
        protected override void DeleteCache() => DeleteFile(FilePath);
    }
}
