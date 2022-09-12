using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;

namespace Ouroboros_API
{
    public class Config
    {
        public string playerName;
        public long playerId;
        public bool FCReq = false;
        public bool PlayedReq = false;
        public bool SongSuggest = false;
        public bool SnipeTime = false;
        public int snipeNum = 10;
        public long[] snipeList;

        public Config()
        {

        }

        public Config(long id)
        {
            playerName = GetPlayerInfoBasic(id).name;
            playerId = id;
            snipeList = Array.Empty<long>();
            SaveConfig();
        }

        public static Config LoadConfig(long id)
        {
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Loading config for " + id);
            string data = Load($@"{mainDataPath + id}_config.json");
            if (data == null)
            {
                if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Creating new config for " + id);
                return new Config(id);
            }
            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Loaded config:\n" + data);
            return DeserializeString<Config>(data);
        }

        public void SaveConfig()
        {
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Saving config for " + playerId);
            string data = SerializeToJSON(this);
            Save($@"{mainDataPath + playerId}_config.json", data);
            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Saved config:\n" + data);
        }
    }
}
