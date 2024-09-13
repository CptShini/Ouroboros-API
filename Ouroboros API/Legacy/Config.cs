using System;
using static Ouroboros_API.Legacy.Core;
using static Ouroboros_API.Legacy.Queries;

namespace Ouroboros_API.Legacy
{
    public class Config
    {
        public string PlayerName;
        public long PlayerId;
        public bool GenerateReqPlaylists = true;
        public bool FcReq = true;
        public bool PlayedReq = false;
        public bool ReqPlaylistsSortByAge = true;
        public bool SplitByElderTech = true;
        public bool ReqPlaylistsOnlyFCs = false;
        public bool SongSuggest = false;
        public bool SsRemoveAlreadyBeat = true;
        public bool SnipeTime = false;
        public int SnipeNum = 10;
        public bool GenerateSnipeList = true;
        public long[] SnipeList;
        public bool DominancePlaylist = false;
        public bool SavePlayGraph = true;

        private Config()
        {
            
        }
        
        private Config(long id)
        {
            PlayerName = GetPlayerInfoBasic(id).name;
            PlayerId = id;
            SnipeList = Array.Empty<long>();
            SaveConfig();
        }

        public static bool LoadConfig(long id, out Config config)
        {
            DebugPrint(DebugLevel.Full, $"Loading config for {id}");
            bool hasConfig = Load($@"{userDataPath + id}_config.json", out string data);
            if (hasConfig)
            {
                DebugPrint(DebugLevel.Dev, $"Loaded config:\n{data}");
                config = DeserializeString<Config>(data);
                return true;
            }

            DebugPrint(DebugLevel.Advanced, $"Creating new config for {id}");
            try
            {
                config = new(id);
                return true;
            }
            catch (Exception)
            {
                DebugPrint(DebugLevel.Advanced, $"Player with id {id} does not exist");
                config = null;
                return false;
            }
        }

        private void SaveConfig()
        {
            DebugPrint(DebugLevel.Full, $"Saving config for {PlayerId}");
            string data = SerializeToJSON(this);
            Save($@"{userDataPath + PlayerId}_config.json", data);
            DebugPrint(DebugLevel.Dev, $"Saved config:\n{data}");
        }
    }
}
