using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;

namespace Ouroboros_API
{
    /// <summary>
    /// A library containing all underlying core functions used to operate, communicate, and handle aspects of the ScoreSaber API.
    /// </summary>
    public static class Core
    {

        private static int sleepTime = 0;

        #region API & Save Data stuff

        /// <summary>
        /// The part of the URL that is present in every Get-Call to the API.
        /// </summary>
        private const string APIBaseUrl = "https://scoresaber.com/api/";
        /// <summary>
        /// The Web Client used to contact the ScoreSaber API.
        /// </summary>
        private static readonly WebClient client = new();
        /// <summary>
        /// Some basic settings to stop the JSON deserializer from returning errors when a value gotten from the API is null.
        /// </summary>
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// The players Beat Saber playlist directory. Example: E:\Steam\steamapps\common\Beat Saber\Playlists\
        /// </summary>
        private static string playlistPath;
        /// <summary>
        /// The path to the folder in which all API data is saved for later use.
        /// </summary>
        public const string mainDataPath = @"SaveLoadData\";
        /// <summary>
        /// A dictionary that where key is the requested URL (only the section after APIBaseURL), and the value is the requested data.
        /// </summary>
        private static readonly Dictionary<string, string> TempSaveData = new();

        #endregion

        #region Initialization

        /// <summary>
        /// Sets the directory to the players playlist folder and sets up the neccessary folders.
        /// </summary>
        /// <param name="playlistPath">The path to the players Beat Saber playlist directory.</param>
        public static void Initialize(string playlistPath)
        {
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Initializing Program");
            Core.playlistPath = playlistPath + @"0uroboros\";
            CreateRequiredFolders();
        }

        /// <summary>
        /// Creates the required folders for the program to work, unless already made.
        /// </summary>
        private static void CreateRequiredFolders()
        {
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Creating required folders if they do not already exist");
            Directory.CreateDirectory(mainDataPath);
            Directory.CreateDirectory(playlistPath + @"#\");
            Directory.CreateDirectory(playlistPath + @"Sniping\");
            Directory.CreateDirectory(playlistPath + @"Øuroboros\");
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets requested data from URL. First attempts to get data from buffer (saves data called during runtime, so program never makes the same request twice), 
        /// secondly attempts to load data from local storage. If neither work, it makes the API request and saves the data locally for later use.
        /// </summary>
        /// <param name="url">The request url after the https://scoresaber.com/api/. Example: player/76561198074878770/basic</param>
        /// <param name="shouldAttemptLoadData">Whether or not the program should attempt to load any local data on the computer.</param>
        /// <param name="assumedTotalCount">The supposed total number of items in the requested collection; used to detect if data is out of sync/too old. Only usable with collections!</param>
        /// <returns>A string containing the requested data in a JSON format.</returns>
        public static string GetContents(string url, bool shouldAttemptLoadData, int assumedTotalCount)
        {
            if (!TempSaveData.TryGetValue(url, out string result))
            {
                bool getNewData = !(shouldAttemptLoadData && TryLoadData(url, assumedTotalCount, out result));
                if (getNewData)
                {
                    result = APIGet(url);
                    SaveAPIData(url, result);
                }
                TempSaveData.Add(url, result);
            }
            return result;
        }

        /// <summary>
        /// Downloads data from the requested URL via the ScoreSaber API.
        /// </summary>
        /// <param name="url">The request url after the https://scoresaber.com/api/. Example: player/76561198074878770/basic</param>
        /// <returns>A string containing the requested data in a JSON format.</returns>
        public static string APIGet(string url)
        {
            Stopwatch timer = new();
            timer.Start();
            string result = client.DownloadString(APIBaseUrl + url);
            if (sleepTime > 0) System.Threading.Thread.Sleep(sleepTime);
            timer.Stop();

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine($"Get-Request ({timer.ElapsedMilliseconds}ms): {APIBaseUrl}{url}");
            return result;
        }

        #endregion

        #region Playlist Creation Functions

        public static void GenerateEmptyPlaylists(int n, string path)
        {
            for (int i = 0; i < n; i++)
            {
                GenerateEmptyPlaylist($"placeholder {i}", path);
            }
        }

        private static void GenerateEmptyPlaylist(string title, string path)
        {
            GenerateBPList(title, path, Array.Empty<LeaderboardInfo>());
        }

        /// <summary>
        /// Creates a playlist inside the given path for a given collection of maps.
        /// </summary>
        /// <param name="title">The title of the playlist displayed in game.</param>
        /// <param name="path">The local path inside the Beat Saber playlist directory. Example Beat Saber\Playlists\"Sniping\"...playlists.</param>
        /// <param name="maps">The array of maps to convert into a playlist.</param>
        /// <returns>True, if playlist generation was successful. False, if playlist wasn't generated.</returns>
        public static bool GenerateBPList(string title, string path, LeaderboardInfo[] maps)
        {
            if (maps.Length <= 0 && !title.Contains("placeholder")) { if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Failed to create playlist. Maps array of length 0"); return false; }
            
            string contents = GetPlaylistString(title, "Ouroboros", maps);
            string salt = $" ({Base64Encode(DateTime.Now.Millisecond.ToString())})";
            title = CleanFileName(title + salt);
            path = playlistPath + path + title + ".bplist";

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Creating playlist called " + title + " with length " + maps.Length);
            Save(path, contents);
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Finished creating playlist");
            return true;
        }

        /// <summary>
        /// Creates the playlist string for a collection of maps.
        /// </summary>
        /// <param name="title">The title of the playlist displayed in game.</param>
        /// <param name="author">The name of the author of the playlist.</param>
        /// <param name="maps">The array of maps to convert into a playlist.</param>
        /// <returns>The full, formatted playlist string for a given array of maps; ready to be put directly into a file with end .bplist.</returns>
        private static string GetPlaylistString(string title, string author, LeaderboardInfo[] maps)
        {
            string result = "";
            result += "{\n";

            result += "  \"playlistTitle\": \"" + title.Replace(@"\", "") + "\",\n";
            result += "  \"playlistAuthor\": \"" + author + "\",\n";
            result += "  \"image\": \"\",\n";

            //data:image/png;base64,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
            result += "  \"songs\": [\n";

            foreach (LeaderboardInfo map in maps)
            {
                result += GetSongString(map);
            }

            result = result.TrimEnd(new char[2] { ',', '\n'});
            result += "\n  ]\n";
            result += "}";

            return result;
        }

        /// <summary>
        /// Creates the playlist song string for given map. (Warning: includes a comma at the end remember to remove for last song in playlist!)
        /// </summary>
        /// <param name="map">The leaderboard to get the given song string for.</param>
        /// <returns>The song string for given map.</returns>
        private static string GetSongString(LeaderboardInfo map)
        {
            string result = "";
            result += "    {\n";

            result += "      \"hash\": \"" + map.songHash + "\",\n";
            result += "      \"difficulties\": [\n";
            result += "        {\n";
            result += "          \"characteristic\": \"Standard\",\n";
            result += "          \"name\": \"" + ResolveDifficultyName(map.difficulty.difficulty) + "\"\n";
            result += "        }\n";
            result += "      ]\n";
            result += "    },\n";

            return result;
        }

        /// <summary>
        /// Removes any illegal characters from the filename.
        /// </summary>
        /// <param name="fileName">The file name to be cleaned.</param>
        /// <returns>The cleaned file name.</returns>
        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        /// <summary>
        /// Deletes all playlists in the predetermined folders.
        /// </summary>
        public static void DeletePlaylists()
        {
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Clearing playlist folder");
            DeleteFolderContents(playlistPath + @"#\", "bplist");
            DeleteFolderContents(playlistPath + @"Sniping\", "bplist");
            DeleteFolderContents(playlistPath + @"Øuroboros\", "bplist");
        }

        #endregion

        #region Special Functions

        #region Resolve

        /// <summary>
        /// Calculates the % PP gains for a given accuracy using PP curve V3.
        /// </summary>
        /// <param name="acc">A given accuracy. Example: 94.34%.</param>
        /// <returns>The % gains for a given accuracy. (as a decimal number, 1 = 100%)</returns>
        public static float CurveV3(float acc)
        {
            acc /= 100f;

            float gain = 0f;
            float[][] curveV3 = GetCurveV3();
            for (int i = 0; i < curveV3.Length; i++)
            {
                if (acc <= curveV3[i][0])
                {
                    float val = (acc - curveV3[i - 1][0]) / (curveV3[i][0] - curveV3[i - 1][0]);
                    gain = curveV3[i - 1][1] * (1 - val) + curveV3[i][1] * val;
                    break;
                }
            }

            return gain;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The float values pairs representing the ScoreSaber pp curve V3.</returns>
        private static float[][] GetCurveV3()
        {
            float[][] curveV3 = new float[19][];

            curveV3[18] = new float[2] { 1, 1.5f };
            curveV3[17] = new float[2] { 0.99f, 1.39f };
            curveV3[16] = new float[2] { 0.98f, 1.29f };
            curveV3[15] = new float[2] { 0.97f, 1.2f };
            curveV3[14] = new float[2] { 0.96f, 1.115f };
            curveV3[13] = new float[2] { 0.95f, 1.046f };
            curveV3[12] = new float[2] { 0.945f, 1.015f };
            curveV3[11] = new float[2] { 0.925f, 0.905f };
            curveV3[10] = new float[2] { 0.9f, 0.78f };
            curveV3[9] = new float[2] { 0.86f, 0.6f };
            curveV3[8] = new float[2] { 0.8f, 0.42f };
            curveV3[7] = new float[2] { 0.75f, 0.3f };
            curveV3[6] = new float[2] { 0.7f, 0.22f };
            curveV3[5] = new float[2] { 0.65f, 0.15f };
            curveV3[4] = new float[2] { 0.6f, 0.105f };
            curveV3[3] = new float[2] { 0.55f, 0.06f };
            curveV3[2] = new float[2] { 0.5f, 0.03f };
            curveV3[1] = new float[2] { 0.45f, 0.015f };
            curveV3[0] = new float[2] { 0f, 0f };

            return curveV3;
        }

        public static int GetPlaceholderNum(int n)
        {
            return (n % 5) > 0 ? 5 - (n % 5) : 0;
        }
        
        public static float CalculatePPGainForRaw(float pp, PlayerScore[] scores)
        {
            PlayerScore ppScore = new PlayerScore();
            ppScore.score = new Score();
            ppScore.score.pp = pp;

            return CalculatePP(scores.Append(ppScore).OrderByDescending(ps => ps.score.pp).ToArray()) - CalculatePP(scores);
        }

        public static float CalculatePPGainForRaw(PlayerScore[] snipeScores, PlayerScore[] scores)
        {
            List<PlayerScore> ppScoresList = new List<PlayerScore>();

            foreach (PlayerScore newScore in snipeScores)
            {
                ppScoresList.Add(newScore);
            }

            foreach (PlayerScore score in scores)
            {
                if (!ppScoresList.Any(ps => ps.leaderboard.id == score.leaderboard.id)) ppScoresList.Add(score);
            }

            return CalculatePP(ppScoresList.OrderByDescending(ps => ps.score.pp).ToArray()) - CalculatePP(scores);
        }

        public static float CalculatePPRawForGain(float desiredGlobalPP, PlayerScore[] scores)
        {
            float temp = 1f;
            float error = 100;
            while (error > 0.01f)
            {
                error = desiredGlobalPP - CalculatePPGainForRaw(temp, scores);
                
            }
            return temp;
        }

        /// <summary>
        /// Calculates the amount of PP for a given set of scores.
        /// </summary>
        /// <param name="scores">The array of scores from which to calculate the PP.</param>
        /// <returns>The amount of PP a player would if they ONLY played the given scores.</returns>
        public static float CalculatePP(PlayerScore[] scores)
        {
            float pp = 0f;
            for (int i = 0; i < scores.Length; i++)
            {
                pp += scores[i].score.pp * MathF.Pow(0.965f, i);
            }

            return pp;
        }

        public static float GetAccuracy(LeaderboardInfo leaderboard, Score score)
        {
            return (float)score.baseScore / leaderboard.maxScore * 100;
        }

        public static float GetRelativeRank(LeaderboardInfo leaderboard, Score score)
        {
            return ((float)score.rank - 1) / leaderboard.plays * 100;
        }

        public static string GetSongNameWDiff(LeaderboardInfo leaderboard)
        {
            return leaderboard.songName + " (" + ResolveDifficultyName(leaderboard.difficulty.difficulty) + ")";
        }

        public static PlayerScore[] UpdatePlayerScores(PlayerScore[] playerScores)
        {
            List<PlayerScore> result = playerScores.Where(ps => ps.leaderboard.ranked).ToList();

            result.ForEach(ps => ps.accuracy = GetAccuracy(ps.leaderboard, ps.score));
            result.ForEach(ps => ps.relativeRank = GetRelativeRank(ps.leaderboard, ps.score));
            result.ForEach(ps => ps.leaderboard.songNameWDiff = GetSongNameWDiff(ps.leaderboard));

            return result.ToArray();
        }

        /// <summary>
        /// Calculates the average accuracy for a given set of scores.
        /// </summary>
        /// <param name="scores">The array of scores from which to calculate average accuracy.</param>
        /// <returns>The average accuracy amongst the given scores.</returns>
        public static float GetAverageAcc(PlayerScore[] scores)
        {
            float accSum = 0f;
            float weightSum = 0f;

            foreach (PlayerScore score in scores)
            {
                accSum += score.accuracy * score.score.weight;
                weightSum += score.score.weight;
            }

            return accSum / weightSum;
        }

        /// <summary>
        /// Resolves which number is smallest as to never attempt to request a greater number of items than is possible.
        /// </summary>
        /// <param name="n">The requested number of items; use -1 for max.</param>
        /// <param name="maxCount">The maximum possible number of items to request.</param>
        /// <returns>The minimum of n and maxCount; returns maxCount if n = -1.</returns>
        public static int NumResolve(int n, int maxCount)
        {
            return (maxCount > n && n != -1) ? n : maxCount;
        }

        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static float Distance(float a, float b)
        {
            return MathF.Sqrt(a * a + b * b);
        }

        public static int LastXNumbers(long i, int l)
        {
            string s = i.ToString();
            return Convert.ToInt32(s[Math.Max(0, s.Length - l)..]);
        }

        /// <summary>
        /// Resolves which data type the given URL is requesting.
        /// </summary>
        /// <param name="targetURL">The request url after the https://scoresaber.com/api/. Example: player/76561198074878770/basic</param>
        /// <returns>A string of what data type it is, possible types are as follows: leaderboards, leaderboard, players, playerInfo, playerScores.</returns>
        private static string ResolveTargetURLDataType(string targetURL)
        {
            string[] targetURLChunks = targetURL.Split(new[] { '/', '?' }, 3);
            string dataType = targetURLChunks[0];
            dataType += dataType == "player" ? (targetURLChunks[2] == "basic" || targetURLChunks[2] == "full") ? "Info" : "Scores" : "";
            return dataType;
        }

        /// <summary>
        /// Resolves what the name of a given difficulty is. (1 = Easy, 3 = Normal, 5 = Hard, 7 = Expert, 9 = Expert+)
        /// </summary>
        /// <param name="diff">The number corrosponding to a given difficulty. (1 = Easy, 3 = Normal, 5 = Hard, 7 = Expert, 9 = Expert+)</param>
        /// <returns>The corrosponding difficulty name.</returns>
        public static string ResolveDifficultyName(int diff)
        {
            diff = (diff + 1) / 2;
            return diff switch
            {
                1 => "Easy",
                2 => "Normal",
                3 => "Hard",
                4 => "Expert",
                5 => "ExpertPlus",
                _ => "Easy",
            };
        }

        /// <summary>
        /// Formats the rank in relation to the highest rank reached so its always sorted correctly.
        /// </summary>
        /// <param name="rank">The rank you wish to get the name of.</param>
        /// <param name="highestReached">The highest reached rank to compare to.</param>
        /// <returns>A string of the rank formatted in relation to the highest rank reached.</returns>
        public static string RankName(int rank, int highestReached)
        {
            int n = highestReached.ToString().Length;

            return n switch
            {
                1 => "#" + rank.ToString("0"),
                2 => "#" + rank.ToString("00"),
                3 => "#" + rank.ToString("000"),
                4 => "#" + rank.ToString("0000"),
                5 => "#" + rank.ToString("00000"),
                6 => "#" + rank.ToString("000000"),
                7 => "#" + rank.ToString("0000000"),
                8 => "#" + rank.ToString("00000000"),
                _ => "#" + rank.ToString("0")
            };
        }

        #region ReqIncrementResolve

        /// <summary>
        /// Resolves what step size to increase the minimum accuracy requirement by.
        /// </summary>
        /// <param name="currentAccReq">The current minimum accuracy.</param>
        /// <returns>The step size to increase the minimum accuracy by.</returns>
        public static float AccReqIncrementResolve(float currentAccReq)
        {
            float r = currentAccReq;
            if (r < 92f) return 1f;
            else if (r < 95f) return 0.5f;
            else if (r < 97f) return 0.25f;
            else if (r < 98f) return 0.2f;
            else if (r < 100f) return 0.1f;
            return 0f;
        }

        /// <summary>
        /// Resolves what step size to decrease the maximum relative rank requirement by.
        /// </summary>
        /// <param name="currentRelRankReq">The current maximum relative rank.</param>
        /// <returns>The step size to decrease the maximum relative rank by.</returns>
        public static float RelRankReqDecrementResolve(float currentRelRankReq)
        {
            float r = currentRelRankReq;
            if (r > 30f) return 5f;
            else if (r > 10f) return 2f;
            else if (r > 5f) return 1f;
            else if (r > 2f) return 0.5f;
            else if (r > 0.1f) return 0.1f;
            return 0f;
        }

        /// <summary>
        /// Resolves what step size to decrease the maximum rank requirement by.
        /// </summary>
        /// <param name="currentRankReq">The current maximum rank.</param>
        /// <returns>The step size to decrease the maximum rank by.</returns>
        public static int RankReqDecrementResolve(int currentRankReq)
        {
            int r = currentRankReq;
            if (r > 1000) return 500;
            else if (r > 300) return 100;
            else if (r > 200) return 50;
            else if (r > 100) return 25;
            else if (r > 50) return 10;
            else if (r > 10) return 5;
            return 1;
        }

        #endregion

        #endregion

        #region Conversion

        public static PlayerScore[] ConvertScoreToPlayerScore(LeaderboardInfo leaderboard, Score[] scores)
        {
            List<PlayerScore> playerScores = new();

            foreach (Score score in scores)
            {
                PlayerScore playerScore = new PlayerScore() { leaderboard = leaderboard, score = score };
                playerScores.Add(playerScore);
            }

            return UpdatePlayerScores(playerScores.ToArray());
        }

        /// <summary>
        /// Filters out all scores not inbetween the star range.
        /// </summary>
        /// <param name="playerScores">The array of scores to filter.</param>
        /// <param name="stars">The star range of which to filter by.</param>
        /// <returns>The filtered scores.</returns>
        public static PlayerScore[] GetPlayerScoresByStars(PlayerScore[] playerScores, StarRange stars)
        {
            return playerScores.Where(ps => ps.leaderboard.stars >= stars.minStars && ps.leaderboard.stars <= stars.maxStars).ToArray();
        }

        /// <summary>
        /// Converts an array of player scores into an array of their respective maps.
        /// </summary>
        /// <param name="scores">The array of scores to convert.</param>
        /// <returns>The converted array of scores as an array of maps.</returns>
        public static LeaderboardInfo[] ConvertPlayerScoreToLeaderboard(PlayerScore[] scores)
        {
            return scores.Select(d => d.leaderboard).ToArray();
        }

        /// <summary>
        /// Converts an array of maps into a dictionary labled by the id of the maps.
        /// </summary>
        /// <param name="scores">The array of scores to convert.</param>
        /// <returns>A dictionary where key is the id of the map, and the value is the corrosponding play.</returns>
        public static Dictionary<long, PlayerScore> ConvertToIdPlayerScoreDictionary(PlayerScore[] scores)
        {
            Dictionary<long, PlayerScore> IdPlayerScoreDict = new();

            foreach (PlayerScore score in scores)
            {
                IdPlayerScoreDict.Add(score.leaderboard.id, score);
            }

            return IdPlayerScoreDict;
        }

        #endregion

        #region Generic Functions

        /// <summary>
        /// Appends two arrays of type T together.
        /// </summary>
        /// <typeparam name="T">The type of array to append</typeparam>
        /// <param name="x">The first array to append.</param>
        /// <param name="y">The last array to append.</param>
        /// <returns>The two arrays converted into one. [x->y]</returns>
        public static T[] AppendArrays<T>(T[] x, T[] y)
        {
            T[] z = new T[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);
            return z;
        }

        /// <summary>
        /// Conjoins all pages gotten from the pageFunction.
        /// </summary>
        /// <typeparam name="T">The type of array to return.</typeparam>
        /// <param name="pageLength">The length of each page.</param>
        /// <param name="itemAmount">The total amount of items.</param>
        /// <param name="startingPage">The page at which the algorithm starts.</param>
        /// <param name="pageFunction">The function that actually gets each individual page.</param>
        /// <returns>An array of type T containing all of the pages contents joined together.</returns>
        public static T[] LoopOverPages<T>(int pageLength, int itemAmount, int startingPage, Func<int, T[]> pageFunction)
        {
            int pageCount = itemAmount % pageLength > 0 ? itemAmount / pageLength + 1 : itemAmount / pageLength;

            List<T> itemList = new();
            for (int i = startingPage; i < pageCount + 1; i++)
            {
                T[] itemPage = pageFunction(i);
                for (int j = 0; j < (i < pageCount ? pageLength : itemAmount - ((pageCount - 1) * pageLength)); j++)
                {
                    itemList.Add(itemPage[j]);
                }
            }

            return itemList.ToArray();
        }

        #endregion

        #endregion

        #region JSON Conversion

        /// <summary>
        /// Converts given object of type T into a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="contents">The object to convert to a JSON string.</param>
        /// <returns>The converted object as a string.</returns>
        public static string SerializeToJSON<T>(T contents)
        {
            return JsonConvert.SerializeObject(contents, serializerSettings);
        }

        /// <summary>
        /// Converts a JSON formatted string into an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of object to parse the string into.</typeparam>
        /// <param name="contents">The JSON formatted string to be converted into object of type T.</param>
        /// <returns>An object of type T.</returns>
        public static T DeserializeString<T>(string contents)
        {
            return JsonConvert.DeserializeObject<T>(contents, serializerSettings);
        }

        #endregion

        #region Save & Load

        #region Core

        /// <summary>
        /// Saves the given string at a given path.
        /// </summary>
        /// <param name="path">The full path at which to save the string.</param>
        /// <param name="contents">The actual string to be saved.</param>
        public static void Save(string path, string contents)
        {
            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Saving file at " + path);
            using StreamWriter sw = File.CreateText(path);
            sw.WriteLine(contents);
        }

        public static void SaveAPIData(string url, string result)
        {
            Save($"{mainDataPath}{Base64Encode(url)}.txt", result);
        }

        /// <summary>
        /// Loads data from text file at given path.
        /// </summary>
        /// <param name="path">The full path from which to load data.</param>
        /// <returns>The data loaded from the text file.</returns>
        public static string? Load(string path)
        {
            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Loading file from " + path);
            if (!File.Exists(path))
            {
                if (debugLevel >= DebugLevel.Full) Console.WriteLine("No file exists at " + path);
                return null;
            }
            return File.ReadAllText(path);
        }

        #endregion

        #region SaveData

        /// <summary>
        /// Attempts to load data corrosponding to the target URL.
        /// </summary>
        /// <param name="targetURL">The request url after the https://scoresaber.com/api/. Example: player/76561198074878770/basic</param>
        /// <param name="assumedTotalCount">The supposed total number of items in the requested collection; used to detect if data is out of sync/too old. Only usable with collections!</param>
        /// <param name="result">Assigns successfully loaded data to parameter.</param>
        /// <returns>True, if data was successfully loaded into result string. False, if data was out of sync, too old, or simply not found.</returns>
        private static bool TryLoadData(string targetURL, int assumedTotalCount, out string result)
        {
            bool loadedData = false;
            result = null;

            DirectoryInfo di = new DirectoryInfo(mainDataPath);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name == Base64Encode(targetURL) + ".txt")
                {
                    loadedData = true;
                    string dataType = ResolveTargetURLDataType(targetURL);
                    TimeSpan age = DateTime.Now - file.LastWriteTime;
                    result = Load(mainDataPath + file.Name);
                    
                    switch (dataType)
                    {
                        case "leaderboards":
                            LeaderboardInfoCollection lbic = DeserializeString<LeaderboardInfoCollection>(result);
                            if (lbic.metadata.total != assumedTotalCount) loadedData = false;
                            break;
                        case "leaderboard":
                            if (age > new TimeSpan(240, 0, 0)) loadedData = false;
                            break;
                        case "players":
                            PlayerCollection pc = DeserializeString<PlayerCollection>(result);
                            //if (pc.metadata.total != assumedTotalCount) loadedData = false;
                            /*if (age > new TimeSpan(24, 0, 0))*/ loadedData = false;
                            break;
                        case "playerInfo":
                            /*if (age > new TimeSpan(1, 0, 0))*/ loadedData = false;
                            break;
                        case "playerScores":
                            PlayerScoreCollection psc = DeserializeString<PlayerScoreCollection>(result);
                            if (LastXNumbers(psc.player.scoreStats.totalRankedScore, 9) != assumedTotalCount) loadedData = false;
                            break;
                    }

                    if (!loadedData)
                    {
                        file.Delete();
                        if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Deleting data! Either out of sync or too old");
                    }
                }
            }

            return loadedData;
        }

        /// <summary>
        /// Renews all API data locally stored on the computer.
        /// </summary>
        public static void RenewAllData()
        {
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Renewing all data");
            DirectoryInfo di = new DirectoryInfo(mainDataPath);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                if (!file.Name.StartsWith("b") && !file.Name.StartsWith("c")) continue;
                string fileName = file.Name.Split(new[] { '.' }, 2)[0];
                string url = Base64Decode(fileName);

                file.Delete();
                GetContents(url, false, -1);
            }
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Finished renewing all data");
        }

        /// <summary>
        /// Deletes all contents at a given path.
        /// </summary>
        /// <param name="path">The full path at which to delete all contents.</param>
        public static void DeleteFolderContents(string path, string ext)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                string fileExt = file.Name.Split(new[] { '.' }, 2)[1];
                if (fileExt != ext) continue;

                file.Delete();
            }
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Folder contents deleted successfully");
        }

        public static void ClearAllData(string ext)
        {
            DeleteFolderContents(mainDataPath, ext);
        }

        #endregion

        #region Base 64

        /// <summary>
        /// Encodes string input into base64.
        /// </summary>
        /// <param name="plainText">The input to be encoded</param>
        /// <returns>The base64 encoded input.</returns>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            string encodedText = Convert.ToBase64String(plainTextBytes).Replace('/', '-');
            return encodedText;
        }

        /// <summary>
        /// Decodes base64 input into string.
        /// </summary>
        /// <param name="base64EncodedData">The base64 data to be decoded.</param>
        /// <returns>The string decoded output.</returns>
        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData.Replace('-', '/'));
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        #endregion

        #endregion

        #region Print Functions

        public static void Print(dynamic contents)
        {
            Console.WriteLine(contents);
        }

        #region Leaderboards

        public static void PrintLeaderboardInfos(LeaderboardInfo[] lbis)
        {
            foreach (LeaderboardInfo lbi in lbis)
            {
                PrintLeaderboardInfo(lbi);
            }
        }

        public static void PrintLeaderboardInfo(LeaderboardInfo lb)
        {
            Console.Write($"{AWSAS($"{lb.stars:0.00}*", 7)}\t | ");
            Console.Write($"{AWSAS(lb.maxScore.ToString(), 9)}\t | ");
            Console.Write($"{Truncate(lb.rankedDate.ToString(), 10)}\t | ");
            Console.Write($"{AWSAE(ResolveDifficultyName(lb.difficulty.difficulty), 10)}\t | ");
            Console.Write($"{AWSAE(Truncate(lb.levelAuthorName, 20), 20)}\t | ");
            Console.Write($"{AWSAE(Truncate(lb.songAuthorName, 20), 20)}\t | ");
            Console.Write($"{lb.songName} {(lb.songSubName.Length != 0 ? $"{lb.songSubName} " : "")}");
            Console.WriteLine();
        }

        #endregion

        #region Scores

        public static void PrintScores(Score[] scores)
        {
            for (int i = 0; i < scores.Length; i++)
            {
                Console.Write($"{(i + 1):00}#\t |");
                PrintScore(scores[i]);
            }
        }

        private static void PrintScore(Score score)
        {
            Console.Write($"{score.pp:0.00}pp\t | ");
            Console.Write($"{AWSAE(score.fullCombo ? "FC" : $"{score.maxCombo} ({score.missedNotes + score.badCuts}x)", 10)}\t ");
            Console.Write($"{AWSAE($"{AWSAS($"{score.baseScore}", 7)}{(score.modifiers.Length != 0 ? $" {score.modifiers}" : "")}", 13)} | ");
            Console.Write($"{AWSAS($"{(DateTime.Now - score.timeSet).TotalDays:0.0} days", 10)}\t | ");
            Console.Write($"{score.leaderboardPlayerInfo.name}");
            Console.WriteLine();
        }

        #endregion

        #region Players

        public static void PrintPlayers(Player[] players)
        {
            foreach (Player player in players)
            {
                PrintPlayer(player);
            }
        }

        private static void PrintPlayer(Player player)
        {
            Console.Write($"{AWSAS($"{player.rank}#", 7)} {AWSAS($"({player.countryRank}#", 6)}) | ");
            Console.Write($"{player.scoreStats.averageRankedAccuracy:00.00}% | {AWSAS($"{player.scoreStats.rankedPlayCount}", 5)} | ");
            Console.Write($"{AWSAS($"{player.pp:0}pp", 8)} | {player.name}");
            Console.WriteLine();
        }

        #endregion

        #region PlayerScores

        public static void PrintPlayerScores(PlayerScore[] scores)
        {
            foreach (PlayerScore score in scores)
            {
                PrintPlayerScore(score);
            }
        }

        public static void PrintPlayerScore(PlayerScore playerScore)
        {
            Score score = playerScore.score;
            LeaderboardInfo leaderboard = playerScore.leaderboard;

            Console.Write($"{AWSAS($"{leaderboard.stars:0.00}*", 7)}\t | {score.pp:0.00}pp ({score.pp * score.weight:0.00}pp)\t | ");
            Console.Write($"{AWSAE(score.fullCombo ? "FC" : $"{score.maxCombo} ({score.missedNotes + score.badCuts}x)", 12)}");
            Console.Write($"{AWSAE($"{AWSAS($"{score.baseScore}", 7)}{(score.modifiers.Length != 0 ? $" {score.modifiers}" : "")}", 13)} | "); //
            Console.Write($"{AWSAS($"{playerScore.accuracy:00.00}%", 7)} | {AWSAS($"{score.rank}#", 6)}  {AWSAS($"{playerScore.relativeRank:0.00}#%", 7)}   | "); //  {AWSAS($"{playerScore.relativeAccuracy:00.00}%%", 8)}
            Console.Write($"{AWSAS($"{(DateTime.Now - score.timeSet).TotalDays:0.0} days", 10)} | ");
            Console.Write($"{AWSAE(ResolveDifficultyName(leaderboard.difficulty.difficulty), 10)}\t | ");
            Console.Write($"{leaderboard.songName} {(leaderboard.songSubName.Length != 0 ? $"{leaderboard.songSubName} " : "")}");
            Console.WriteLine();
        }

        #endregion

        private static string AWSAS(string s, int desiredLength)
        {
            string result = "";
            for (int i = 0; i < desiredLength - s.Length; i++)
            {
                result += " ";
            }
            return result + s;
        }
        
        private static string AWSAE(string s, int desiredLength)
        {
            string result = "";
            for (int i = 0; i < desiredLength - s.Length; i++)
            {
                result += " ";
            }
            return s + result;
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        #endregion

    }
}