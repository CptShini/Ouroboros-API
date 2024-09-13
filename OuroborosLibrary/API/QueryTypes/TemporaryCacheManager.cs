using static OuroborosLibrary.SaveLoad.FileSaveLoadManager;

namespace OuroborosLibrary.API.QueryTypes
{
    internal class TemporaryCacheManager : QueryCacheManager
    {
        internal TemporaryCacheManager(string targetURL, bool saveable) : base(targetURL, saveable) { }

        protected override void Cache(string queryResponse) => SaveFile(FilePath, queryResponse);
        protected override bool LoadCache(out string cache) => LoadFile(FilePath, out cache);
        protected override bool IsCacheUpToDate(string loadedCache) => false;
        protected override void DeleteCache() => DeleteFile(FilePath);
    }
}
