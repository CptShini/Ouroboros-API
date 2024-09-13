namespace Ouroboros_API.Legacy;

internal class Settings
{
    public string BeatSaberPath;
    public long DefaultPlayerId;
    public DebugLevel DebugLevel;

    public Settings(string path, long playerId, DebugLevel debugLevel)
    {
        BeatSaberPath = path;
        DefaultPlayerId = playerId;
        DebugLevel = debugLevel;
    }
}