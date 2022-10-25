using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using static Ouroboros_API.Sniping;
using static Ouroboros_API.Playlists;
using static Ouroboros_API.Config;
using System;
using System.Linq;

namespace Ouroboros_API
{
    class Program
    {

        #region Variables & Main

        #region IDs

        public const long CptShini = 76561198074878770;
        public const long Sharkz = 76561198089913211;
        public const long Sensei = 76561198400393482;
        public const long HollowHuu = 76561198104292086;
        public const long Kat = 76561197997028786;
        public const long Zorri = 76561198163644494;
        public const long Jonathan = 76561198216194272;
        public const long Eff3ct = 76561198188430027;
        public const long Guy = 76561198136806348;
        public const long Viking = 76561198299618436;
        public const long Coreh = 76561198198217778;
        public const long Dark = 76561198030407451;
        public const long Hawk = 76561198086326146;
        public const long Lasse = 76561198113078478;
        public const long Soulless = 76561198089234369;
        public const long Taoh = 76561197993806676;
        public const long Tseska = 76561198362923485;
        public const long CaraX = 76561198118927554;
        public const long RoboDK = 76561197970445748;
        public const long Wahlb3rg = 76561198116981578;

        #endregion

        static void Main(string[] args)
        {
            Program program = new();
            program.Start();
            Console.ReadKey();
        }

        #endregion

        void Start()
        {
            Initialize(@"E:\Steam\steamapps\common\Beat Saber\");
            GenerateOuroborosSet(CptShini);
        }

        private static void GenerateOuroborosSet(long id)
        {
            ClearData(DataType.Playlists);

            LoadConfig(id);

            Player player = GetPlayerInfoFull(config.playerId);
            GenerateOuroboros(0, 14);
            GenerateTopPlaysPlaylist(player);
            GenerateReqPlaylists(player, 0, 14);
            int n = 0;

            if (config.snipeList.Length > 0) n += GenerateSnipeTargetsPlaylists(player);

            if (config.SnipeTime) n += GenerateSnipeTimePlaylists(player, false, config.snipeNum, false);
            if (config.SongSuggest) GenerateSongSuggest(player, true);
            GenerateEmptyPlaylists(n, "Sniping", @"Sniping\");

            if (config.dominancePlaylist) GenerateDominancePlaylist(player);
            if (config.savePlayGraph) GeneratePlayGraphs();

            if (debugLevel >= DebugLevel.None) Println("Ouroboros has finished running");
        }

        private static void PrintPlayerScoresAndLeaderboards(long id)
        {
            Player player = GetPlayerInfoFull(id);
            PlayerScore[] scores = GetPlayerScores(player, -1);

            LeaderboardInfo[] leaderboards = GetLeaderboards(new StarRange(0, 14), 0);
            PrintScoreLeaderboardMix(scores, leaderboards);
        }

        private static void GetPrintPlayerScores(long id, bool sortByStars)
        {
            Player player = GetPlayerInfoFull(id);
            PlayerScore[] scores = GetPlayerScores(player, -1).OrderByDescending(ps => sortByStars ? ps.leaderboard.stars : ps.accuracy).ToArray();
            PrintPlayerScores(scores);
        }

        public static float GetAverageAccAtStars(float starVal, PlayerScore[] scores)
        {
            float weightedAccSum = 0f, weightSum = 0f;
            for (int j = 0; j < scores.Length; j++)
            {
                float starDiff = scores[j].leaderboard.stars - starVal;
                starDiff = starDiff < 0 ? -starDiff : starDiff;

                float songWeight = 1f / (3f * starDiff + (1f / (1f + 0.2f))) - 0.2f;
                songWeight = songWeight < 0 ? 0f : songWeight;

                weightedAccSum += scores[j].accuracy * songWeight;
                weightSum += songWeight;
            }

            float avgAcc = weightSum >= 1f ? weightedAccSum / weightSum : 0f;
            //float avgAcc = weightedAccSum / weightSum;

            Println($"{starVal:00.0}* | {avgAcc:00.00}% | {weightedAccSum} | {weightSum}");
            return avgAcc;
        }
    }
}