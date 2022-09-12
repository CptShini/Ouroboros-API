using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using static Ouroboros_API.Sniping;
using static Ouroboros_API.Playlists;
using static Ouroboros_API.Config;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Ouroboros;
using Newtonsoft.Json;
using System.Drawing;

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
            Initialize(@"E:\Steam\steamapps\common\Beat Saber\Playlists\");
            DeletePlaylists();
            GenerateOuroborosSet(CptShini);
        }

        private static void GenerateOuroborosSet(long id)
        {
            Config con = LoadConfig(id);

            Player player = GetPlayerInfoFull(con.playerId);
            GenerateOuroboros(0, 14);
            GenerateTopPlaysPlaylist(player);
            GenerateReqPlaylists(player, 0, 14, con.FCReq, con.PlayedReq, true);
            int n = 0;
            for (int i = 0; i < con.snipeList.Length; i++)
            {
                if (GenerateSnipeTimePlaylistByID(con.playerId, con.snipeList[i])) n++;
            }
            int k = n;
            if (con.SnipeTime) k += GenerateSnipeTimePlaylists(player, false, con.snipeNum, false);
            if (con.SongSuggest) GenerateSongSuggest(player, true);
            GenerateEmptyPlaylists(GetPlaceholderNum(k), @"Sniping\");
            PrintPlayGraphBMP(player);
        }

        private static void GetPrintPlayerScores(long id, bool sortByStars)
        {
            Player player = GetPlayerInfoFull(id);
            PlayerScore[] scores = GetPlayerScores(player, -1).OrderByDescending(ps => sortByStars ? ps.leaderboard.stars : ps.accuracy).ToArray();
            PrintPlayerScores(scores);
        }

        private static void PrintPlayGraphBMP(Player player)
        {
            PlayerScore[] scores = GetPlayerScores(player, -1).Where(s => s.accuracy >= 80f).ToArray();
            Bitmap bmp = new Bitmap(1000, 500);

            for (int i = 16; i < 20; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    bmp.SetPixel(j, (int)Remap(i, 16, 20, bmp.Height - 1, 0), Color.Gray);
                }
            }

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    bmp.SetPixel((int)Remap(i, 0, 13, 0, bmp.Width - 1), j, Color.Gray);
                }
            }

            for (int i = 0; i < scores.Length; i++)
            {
                float acc = scores[i].accuracy;
                float stars = scores[i].leaderboard.stars;

                int x, y;
                x = (int)Remap(stars, 0, 13, 0, bmp.Width - 1);
                y = (int)Remap(acc, 80, 100, bmp.Height - 1, 0);

                Color c = Color.FromArgb((int)Remap((DateTime.Now - scores[i].score.timeSet).Days, 0, 365, 255, 127), 0, 0);

                if (i <= 10) c = Color.Cyan;
                else if (i <= 24) c = Color.Green;
                else if (i <= 50) c = Color.Orange;

                bmp.SetPixel(x, y, c);
            }

            /*for (int i = 0; i < bmp.Width; i++)
            {
                float starVal = Remap(i, 0f, bmp.Width, 0f, 13f);

                float avgAcc = GetAverageAccAtStars(starVal, scores);
                avgAcc = GetAverageAccAtStars(starVal, scores.Where(ps => ps.accuracy >= avgAcc).ToArray());
                avgAcc = GetAverageAccAtStars(starVal, scores.Where(ps => ps.accuracy >= avgAcc).ToArray());
                
                

                int y = (int)Remap(avgAcc, 80f, 100f, bmp.Height - 1, 0);
                if (y >= bmp.Height || y < 0) continue;
                bmp.SetPixel(i, y, Color.White);
            }*/

            bmp.Save(@$"C:\Users\gabri\Pictures\Beat Saber\{CleanFileName(player.name)}-{player.scoreStats.totalRankedScore}.png");
            Print("Done!");
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

            Print($"{starVal:00.0}* | {avgAcc:00.00}% | {weightedAccSum} | {weightSum}");
            return avgAcc;
        }
    }

    class Map
    {
        public LeaderboardInfo map;

        public int count;

        public List<float> scores;
        public float score;
    }
}