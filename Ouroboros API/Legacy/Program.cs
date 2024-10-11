using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.Legacy.DebugManager;
using static Ouroboros_API.Legacy.Core;
using static Ouroboros_API.Legacy.Queries;
using static Ouroboros_API.Legacy.Sniping;
using static Ouroboros_API.Legacy.Playlists;
using static Ouroboros_API.Legacy.SongSuggest;
using static Ouroboros_API.Legacy.Config;
using System;
using System.Linq;
using System.Net;
using System.Threading;

/*using OuroborosLibrary.ScoreSaberClasses;
using OuroborosLibrary.Queries;
using OuroborosLibrary.Debugging;
using OuroborosLibrary.SaveLoad;*/

namespace Ouroboros_API.Legacy
{
    class Program
    {
        #region Main & Settings

        private static Settings _programSettings;
        
        private static void Main()
        {
            HandleSettings();
            debugLevel = _programSettings.DebugLevel;
            Initialize(_programSettings.BeatSaberPath);
            Start();
            Console.ReadLine();
        }

        private static void HandleSettings()
        {
            bool hasSettings = Load("Settings.json", out string settingsData);
            if (hasSettings)
            {
                _programSettings = DeserializeString<Settings>(settingsData);
                return;
            }
            
            Console.WriteLine("--- Default settings ---\nPlease enter the full path to your install of Beat Saber (should end with: \\Beat Saber):");
            string beatSaberPath = Console.ReadLine() + '\\';
                
            Console.WriteLine("\nPlease enter the ID of the player to load by default. (Best to put your own ID)");
            long playerId = Convert.ToInt64(Console.ReadLine());
            _programSettings = new(beatSaberPath, playerId, DebugLevel.None);

            Save("Settings.json", SerializeToJSON(_programSettings));
            Console.Clear();
        }
        
        #endregion

        private static void Start()
        {
            while (true)
            {
                Console.Write("Please enter player ID (-1 for default): ");
                string input = Console.ReadLine();
                bool wasNumber = long.TryParse(input, out long id);
                if (!wasNumber)
                {
                    if (input!.ToLower().Equals("delete"))
                    {
                        Console.WriteLine("Deleting Ouroboros SaveData... (click any key to continue)");
                        Console.ReadKey();
                        ClearData();
                        Console.WriteLine("Done!");
                    }
                    else Console.WriteLine("Please enter a valid number. :/");
                        
                    Console.ReadLine();
                    Console.Clear();
                    continue;
                }

                if (id == -1) id = _programSettings.DefaultPlayerId;

                bool playerExists = LoadConfig(id, out Core.Config);
                if (!playerExists)
                {
                    Console.WriteLine($"Player with id {id}, did not exist. Please try again!");
                    
                    Console.ReadLine();
                    Console.Clear();
                    continue;
                }

                try
                {
                    GenerateOuroborosSet();
                }
                catch (WebException e)
                {
                    DebugPrint(DebugLevel.Advanced, $"Caught a WebException while Ouroboros was running!\n{e}");
                    HahaFunny();
                }
                break;
            }
            
            /*FilePathManager.InitializeFilePaths(_beatSaberPath);

                Player player = APIQueries.GetPlayerInfo(id);
                PlayerScore[] playerScores = player.GetPlayerScores();
                playerScores.PrintPlayerScores();*/

            /*debugLevel = DebugLevel.Full;
            Player[] players = APIQueries.GetPlayers(1, 5000).Where(p => p.scoreStats.rankedPlayCount >= 1500).ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                PlayerScore[] playerScores = APIQueries.GetPlayerScores(players[i]);
                int FCs = playerScores.Count(ps => ps.score.fullCombo);

                players[i].pp = FCs;
            }

            PrintPlayers(players.OrderBy(p => p.rank).ToArray());
            Println("\n\n");
            PrintPlayers(players.OrderByDescending(p => p.scoreStats.rankedPlayCount).ToArray());
            Println("\n\n");
            PrintPlayers(players.OrderByDescending(p => p.pp).ToArray());*/
        }

        private static void GenerateOuroborosSet()
        {
            ClearData(DataType.Playlists);

            Player player = GetPlayerInfoFull(Core.Config.PlayerId);
            GenerateOuroboros(0, 14);
            GenerateTopPlaysPlaylist(player);
            if (Core.Config.GenerateReqPlaylists) GenerateReqPlaylists(player, 0, 14);
            
            if (Core.Config.SavePlayGraph) GeneratePlayGraphs();

            int n = 0;
            if (Core.Config.GenerateSnipeList) n += GenerateSnipeTargetsPlaylists(player);
            if (Core.Config.SnipeTime) n += GenerateSnipeTimePlaylists(player, false, Core.Config.SnipeNum, false);
            GenerateEmptyPlaylists(n, "Sniping", @"Sniping\");

            if (Core.Config.SongSuggest)
            {
                DebugPrint(DebugLevel.None, "\nSuggesting some banger maps to play");
                GenerateSongSuggest(player, 0.85f);
                GenerateSongSuggest(player, 0.45f);
                GenerateSongSuggest(player, 0.20f);
                GenerateEmptyPlaylists(3, "Ø Song Suggest", @"Sniping\");
            }

            if (Core.Config.DominancePlaylist) GenerateEmptyPlaylists(GenerateDominancePlaylist(player), "X Dominance", @"Øuroboros\");

            DebugPrint(DebugLevel.None, "Ouroboros has finished running");
        }

        private static void HahaFunny()
        {
            Console.Write("Hmm");
            Thread.Sleep(3000);
            PrintThinkingDots(3, 1000);
            Thread.Sleep(2000);
                    
            Console.Write("\nWelp");
            Thread.Sleep(2000);
            Console.Write(" :'D");
            Thread.Sleep(2000);
                    
            Console.Write("\nLong story short");
            Thread.Sleep(2000);
            Console.Write(", some error occured with the ScoreSaber API.");
            Thread.Sleep(3000);
                    
            Console.Write("\nIt was probably because you were sending too many requests...");
            Thread.Sleep(4000);
            Console.Write(" (my fault, not yours)");
            Thread.Sleep(3000);
                    
            Console.Write("\nBut data is cached quite nicely, so you could try?");
            Thread.Sleep(3000);
            Console.Write(" idk");
            Thread.Sleep(1000);
            Console.Write(", restarting the program?");
            Thread.Sleep(3000);
                    
            Console.Write("\nIt would keep all the data we pulled");
            Thread.Sleep(3000);
            Console.Write(", and give the server some time to breathe.");
            Thread.Sleep(5000);

            Console.Write("\nYou're gonna have to start it up yourself tho");
            Thread.Sleep(3000);
            Console.Write(", I can't do that for ya :/");
            Thread.Sleep(5000);
                    
            Console.Write("\nYeah");
            Thread.Sleep(2000);
            PrintThinkingDots(3, 500);
            Thread.Sleep(2000);
            Console.Write(" let's do that");
            PrintThinkingDots(3, 1000);
            Thread.Sleep(3000);
            
            Environment.Exit(0);
            
            return;

            static void PrintThinkingDots(int n, int delay)
            {
                Console.Write('.');
                for (int i = 1; i < n; i++)
                {
                    Thread.Sleep(delay);
                    Console.Write('.');
                }
            }
        }
        
        private static void PrintPlayerScoresAndLeaderboards(long id)
        {
            Player player = GetPlayerInfoFull(id);
            PlayerScore[] scores = Queries.GetPlayerScores(player, -1);

            LeaderboardInfo[] leaderboards = GetLeaderboards(new StarRange(0, 14), 0);
            PrintScoreLeaderboardMix(scores, leaderboards);
        }

        private static void GetPrintPlayerScores(long id, bool sortByStars)
        {
            Player player = GetPlayerInfoFull(id);
            PlayerScore[] scores = Queries.GetPlayerScores(player, -1).OrderByDescending(ps => sortByStars ? ps.leaderboard.stars : ps.accuracy).ToArray();
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

            Core.Println($"{starVal:00.0}* | {avgAcc:00.00}% | {weightedAccSum} | {weightSum}");
            return avgAcc;
        }
    }
}