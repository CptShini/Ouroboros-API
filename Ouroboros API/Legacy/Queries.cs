using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.Legacy.DebugManager;
using static Ouroboros_API.Legacy.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ouroboros_API.Legacy
{
    /// <summary>
    /// A library containing all types of available queries to the ScoreSaber API.
    /// </summary>
    public static class Queries
    {

        /// <summary>
        /// A collection of map IDs no longer on BeatSaver.
        /// </summary>
        private static readonly long[] glitchedMaps = new long[]
        {
            2656,
            2874,
            6085
        };

        private static Dictionary<long, int> incorrectMaxScoreMaps = new Dictionary<long, int>()
        {
            { 9025, 181355 },
            { 9028, 141795 },
            { 9023, 324875 },
            { 9007, 476675 },
            { 11909, 340515 },
            { 59409, 320275 },
            { 59096, 424235 },
            { 18691, 237475 },
            { 18728, 438955 },
            { 4022, 262315 },
            { 3231, 374555 },
            { 2720, 468395 },
            { 40892, 249435 },
            { 2900, 531875 },
            { 2895, 651475 },
            { 29546, 227355 },
            { 50328, 526355 },
            { 50288, 824435 },
            { 8270, 176755 },
            { 30818, 383755 },
            { 41481, 605475 },
            { 58412, 721395 },
            { 58409, 597195 },
            { 21670, 254035 },
            { 21628, 357075 },
            { 17020, 449995 },
            { 6004, 516235 },
            { 40338, 311995 },
            { 23871, 594435 }
        };

        #region Leaderboard Queries

        /// <summary>
        /// Gets all ranked leaderboards within the given star range.
        /// </summary>
        /// <param name="stars">The range of star difficulty the maps must lie in.</param>
        /// <param name="sort">The direction which to sort by star difficulty. (0 = descending, 1 = ascending)</param>
        /// <returns>A collection of all ranked maps within the given star range.</returns>
        public static LeaderboardInfo[] GetLeaderboards(StarRange stars, int sort)
        {
            int minStar = (int)MathF.Round(stars.minStars, MidpointRounding.ToNegativeInfinity);
            int maxStar = (int)MathF.Round(stars.maxStars, MidpointRounding.ToPositiveInfinity);

            DebugPrint(DebugLevel.Advanced, $"Retriving {(sort == 0 ? "decending" : "ascending")} ranked leaderboards between {minStar} and {maxStar} stars");

            LeaderboardInfo[] maps = Array.Empty<LeaderboardInfo>();
            for (int i = minStar; i < maxStar; i++)
            {
                LeaderboardInfoCollection lbic = GetLeaderboardsPage(1, i, i + 1, sort, false, -1);
                LeaderboardInfo[] func(int page) => GetLeaderboardsPage(page, i, i + 1, sort, true, lbic.metadata.total).leaderboards;
                LeaderboardInfo[] lbs = LoopOverPages(lbic.metadata.itemsPerPage, lbic.metadata.total, 2, func);
                lbic.leaderboards = AppendArrays(lbic.leaderboards, lbs);

                maps = (sort == 1) ? AppendArrays(maps, lbic.leaderboards) : AppendArrays(lbic.leaderboards, maps);
            }

            DebugPrint(DebugLevel.Full, "Finished retriving leaderboards");

            maps = maps.Where(m => !glitchedMaps.Any(id => id == m.id)).ToArray();

            List<LeaderboardInfo> mapList = maps.ToList();

            for (int i = 0; i < mapList.Count; i++)
            {
                LeaderboardInfo lb = mapList[i];
                lb.beatmapName = GetSongNameWDiff(lb);

                if (incorrectMaxScoreMaps.ContainsKey(lb.id))
                {
                    lb.maxScore = incorrectMaxScoreMaps[lb.id];
                }
            }

            return mapList.ToArray();
        }

        /// <summary>
        /// Gets a given page of leaderboards with difficulty lying within the given star range.
        /// </summary>
        /// <param name="page">The page of the request.</param>
        /// <param name="minStar">The minimum star difficulty of the requested maps.</param>
        /// <param name="maxStar">The maximum star difficulty of the requested maps.</param>
        /// <param name="sort">The direction which to sort by star difficulty. (0 = descending, 1 = ascending)</param>
        /// <param name="shouldAttemptLoadData">Whether or not the program should attempt to load any local data on the computer.</param>
        /// <param name="assumedTotalCount">The supposed total number of items in the requested collection; used to detect if data is out of sync/too old. Only usable with collections!</param>
        /// <returns>A page of leaderboards in the form of a collection.</returns>
        private static LeaderboardInfoCollection GetLeaderboardsPage(int page, int minStar, int maxStar, int sort, bool shouldAttemptLoadData, int assumedTotalCount)
        {
            DebugPrint(DebugLevel.Full, $"Getting {(sort == 0 ? "decending" : "ascending")} ranked leaderboards between {minStar} and {maxStar} stars page {page}");

            string url = $"leaderboards?ranked=true&category=3&page={page}{(minStar >= 0 ? $"&minStar={minStar}" : "")}{(maxStar >= 0 ? $"&maxStar={maxStar}" : "")}{(sort >= 0 ? $"&sort={sort}" : "")}";
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            DebugPrint(DebugLevel.Dev, $"Finished getting leaderboards page {page}");
            return DeserializeString<LeaderboardInfoCollection>(results);
        }

        /// <summary>
        /// Gets the info for a given leaderboard using its ID.
        /// </summary>
        /// <param name="leaderboardID">The id of the leaderboard.</param>
        /// <returns>The leaderboard's info.</returns>
        public static LeaderboardInfo GetLeaderboardInfo(long leaderboardID)
        {
            DebugPrint(DebugLevel.Advanced, $"Getting leaderboard info for {leaderboardID}");

            string url = $"leaderboard/by-id/{leaderboardID}/info";
            string results = GetContents(url, true, -1);

            DebugPrint(DebugLevel.Full, $"Finished getting leaderboard info for {leaderboardID}");
            return DeserializeString<LeaderboardInfo>(results);
        }

        /// <summary>
        /// Gets n number of top scores on a given leaderboard.
        /// </summary>
        /// <param name="leaderboardID">The ID of the given leaderboard to get scores from.</param>
        /// <param name="n">The number of top scores to get.</param>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="search">The query string to search by for players.</param>
        /// <returns>A collection of top scores on a given leaderboard.</returns>
        public static Score[] GetLeaderboardScores(long leaderboardID, int n, string countryCode, string search)
        {
            DebugPrint(DebugLevel.Full, $"Retriving {(countryCode.Length > 0 ? $"{countryCode} " : "")}leaderboard scores {(search.Length > 0 ? $" with name {search} " : "")}for {leaderboardID}");

            ScoreCollection sc = GetLeaderboardScoresPage(1, leaderboardID, countryCode, search, true, -1);
            sc.metadata.total = NumResolve(n, sc.metadata.total);
            Score[] func(int page) => GetLeaderboardScoresPage(page, leaderboardID, countryCode, search, true, sc.metadata.total).scores;
            Score[] scores = LoopOverPages(sc.metadata.itemsPerPage, sc.metadata.total, 2, func);
            

            DebugPrint(DebugLevel.Dev, $"Finished retriving {sc.metadata.total} leaderboard scores for {leaderboardID}");
            scores = AppendArrays(sc.scores, scores).Take(sc.metadata.total).ToArray();

            return scores;
        }

        /// <summary>
        /// Gets a given page of top scores from the given leaderboard.
        /// </summary>
        /// <param name="page">The page of the request.</param>
        /// <param name="leaderboardID">The ID of the given leaderboard to get scores from.</param>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="search">The query string to search by for players.</param>
        /// <param name="shouldAttemptLoadData">Whether or not the program should attempt to load any local data on the computer.</param>
        /// <param name="assumedTotalCount">The supposed total number of items in the requested collection; used to detect if data is out of sync/too old. Only usable with collections!</param>
        /// <returns>A page of top scores from a given leaderboard in the form of a collection.</returns>
        private static ScoreCollection GetLeaderboardScoresPage(int page, long leaderboardID, string countryCode, string search, bool shouldAttemptLoadData, int assumedTotalCount)
        {
            DebugPrint(DebugLevel.Dev, $"Getting {(countryCode.Length > 0 ? $"{countryCode} " : "")}leaderboard scores {(search.Length > 0 ? $" with name {search} " : "")}for {leaderboardID} page {page}");

            string url = $"leaderboard/by-id/{leaderboardID}/scores?page={page}{(countryCode.Length > 0 ? $"&countries={countryCode}" : "")}{(search.Length > 0 ? $"&search={search}" : "")}";
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            return DeserializeString<ScoreCollection>(results);
        }

        #endregion

        #region Player Queries

        /// <summary>
        /// Gets all players fitting given criteria.
        /// </summary>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="rankFrom">The maximum rank to -get; the small number.</param>
        /// <param name="rankTo">The minimum rank to -get; the big number.</param>
        /// <param name="minAcc">The minimum average ranked accuracy of the player.</param>
        /// <param name="computeAcc">Whether or not to calculate the average ranked accuracy of the players top 50 scores, and use said value instead.</param>
        /// <returns>An array of filtered players fitting the given criteria.</returns>
        public static Player[] GetFilteredPlayers(string countryCode, int rankFrom, int rankTo, float minAcc, float maxAcc, bool computeAcc, int desiredAmount = -1)
        {
            DebugPrint(DebugLevel.Advanced, $"Retriving filtered leaderboards for {(countryCode.Length > 0 ? $"{countryCode} " : "")}ranks {rankFrom} through {rankTo}{(minAcc > 0 ? $" with min. avg. {(computeAcc ? "computed" : "")} acc being {minAcc:00.00}%" : "")}");
            Player[] players = GetPlayersByRank(countryCode, rankFrom, rankTo);
            List<Player> filteredPlayers = new();

            DebugPrint(DebugLevel.Advanced, "Filtering out players that dont fit criteria");
            foreach (Player player in players)
            {
                float acc = computeAcc ? GetAverageAcc(GetPlayerScores(player, 50)) : player.scoreStats.averageRankedAccuracy;
                if (AccWithinRange(acc)) filteredPlayers.Add(player);

                if (desiredAmount > 0 && filteredPlayers.Count >= desiredAmount) break;
            }

            DebugPrint(DebugLevel.Advanced, "Finished filtering out players");
            return filteredPlayers.ToArray();
            
            bool AccWithinRange(float acc) => minAcc <= acc && acc <= maxAcc;
        }

        /// <summary>
        /// Gets all players from a given country between the given ranks.
        /// </summary>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="rankFrom">The maximum rank to -get; the small number.</param>
        /// <param name="rankTo">The minimum rank to -get; the big number.</param>
        /// <returns>All players from the given country between the designated ranks; Uses local rank if country is given, else uses global.</returns>
        public static Player[] GetPlayersByRank(string countryCode, int rankFrom, int rankTo)
        {
            int startingPage = rankFrom / 50 + (rankFrom % 50 == 0 ? 0 : 1);
            PlayerCollection pc = GetPlayersPage(startingPage, countryCode, "", false, -1);
            pc.metadata.total = NumResolve(rankTo, pc.metadata.total);
            
            DebugPrint(DebugLevel.Advanced, $"Retriving {(countryCode.Length > 0 ? $"{countryCode} " : "")}leaderboards for ranks {rankFrom} through {pc.metadata.total}; T-{GetTimeEstimate((pc.metadata.total - rankFrom) / pc.metadata.itemsPerPage, 3000)}");

            Player[] func(int page) => GetPlayersPage(page, countryCode, "", true, pc.metadata.total).players;
            Player[] players = LoopOverPages(pc.metadata.itemsPerPage, pc.metadata.total, startingPage + 1, func);
            players = AppendArrays(pc.players, players);
            players = players.Where(p => pc.metadata.total >= (countryCode == "" ? p.rank : p.countryRank) && rankFrom <= (countryCode == "" ? p.rank : p.countryRank)).ToArray();

            DebugPrint(DebugLevel.Full, "Finished retriving leaderboards by rank");
            return players;
        }

        /// <summary>
        /// Gets top n number of players fitting given criteria.
        /// </summary>
        /// <param name="n">The number of players to get. (pp Top->Bottom)</param>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="search">The query string to search by for players.</param>
        /// <returns>A collection of players fitting given criteria.</returns>
        public static Player[] GetPlayers(int n, string countryCode, string search)
        {
            DebugPrint(DebugLevel.Basic, $"Retriving {n} players{(search.Length > 0 ? $" with name {search} " : "")}{(countryCode.Length > 0 ? $" from {countryCode} " : "")}");

            PlayerCollection pc = GetPlayersPage(1, countryCode, search, false, -1);
            pc.metadata.total = NumResolve(n, pc.metadata.total);
            Player[] func(int page) => GetPlayersPage(page, countryCode, search, true, pc.metadata.total).players;
            Player[] players = LoopOverPages(pc.metadata.itemsPerPage, pc.metadata.total, 2, func);

            DebugPrint(DebugLevel.Advanced, $"Finished retriving {pc.metadata.total} players");
            players = AppendArrays(pc.players, players);
            return players;
        }

        /// <summary>
        /// Gets a given page of players fitting given criteria.
        /// </summary>
        /// <param name="page">The page of the request.</param>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="search">The query string to search by for players.</param>
        /// <param name="shouldAttemptLoadData">Whether or not the program should attempt to load any local data on the computer.</param>
        /// <param name="assumedTotalCount">The supposed total number of items in the requested collection; used to detect if data is out of sync/too old. Only usable with collections!</param>
        /// <returns>A page of players fitting the given criteria.</returns>
        private static PlayerCollection GetPlayersPage(int page, string countryCode, string search, bool shouldAttemptLoadData, int assumedTotalCount)
        {
            DebugPrint(DebugLevel.Full, $"Getting players {(search.Length > 0 ? $"with name {search} " : "")}{(countryCode.Length > 0 ? $"from {countryCode} " : "")}page {page}");

            string url = $"players?page={page}{(countryCode.Length > 0 ? $"&countries={countryCode}" : "")}{(search.Length > 0 ? $"&search={search}" : "")}";
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            DebugPrint(DebugLevel.Dev, $"Finished getting players page {page}");
            return DeserializeString<PlayerCollection>(results);
        }

        /// <summary>
        /// Gets the number of players fulfilling given criteria.
        /// </summary>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="search">The query string to search by for players.</param>
        /// <returns>The number of players fitting the given criteria.</returns>
        public static int GetPlayerCount(string countryCode, string search)
        {
            DebugPrint(DebugLevel.Advanced, $"Getting number of players {(search.Length > 0 ? $"with name {search} " : "")}{(countryCode.Length > 0 ? $"from {countryCode} " : "")}");

            string url = $"players/count?{(countryCode.Length > 0 ? $"&countries={countryCode}" : "")}{(search.Length > 0 ? $"&search={search}" : "")}";
            string results = GetContents(url, true, -1);

            return DeserializeString<int>(results);
        }

        /// <summary>
        /// Gets the information of a given player by their ID. (Doesn't include Player.scoreStats!)
        /// </summary>
        /// <param name="playerID">The ID of the player from whom to get information.</param>
        /// <returns>The information of the given player.</returns>
        public static Player GetPlayerInfoBasic(long playerID)
        {
            DebugPrint(DebugLevel.Full, $"Getting basic player info for player {playerID}");

            string url = $"player/{playerID}/basic";
            string results = GetContents(url, false, -1);

            return DeserializeString<Player>(results);
        }

        /// <summary>
        /// Gets the information of a given player by their ID.
        /// </summary>
        /// <param name="playerID">The ID of the player from whom to get information.</param>
        /// <returns>The information of the given player.</returns>
        public static Player GetPlayerInfoFull(long playerID)
        {
            DebugPrint(DebugLevel.Full, $"Getting full player info for player {playerID}");

            string url = $"player/{playerID}/full";
            string results = GetContents(url, false, -1);

            return DeserializeString<Player>(results);
        }

        /// <summary>
        /// Gets n scores from the given player.
        /// </summary>
        /// <param name="player">The player from which to get scores.</param>
        /// <param name="n">The number of scores to get.</param>
        /// <param name="sort">What to sort by. (top, recent)</param>
        /// <returns>A collection of scores from the given player.</returns>
        public static PlayerScore[] GetPlayerScores(Player player, int n)
        {
            if (player.scoreStats.rankedPlayCount <= 0) return Array.Empty<PlayerScore>();

            int totalCount = NumResolve(n, player.scoreStats.rankedPlayCount);
            DebugPrint(DebugLevel.Full, $"Retriving {(totalCount > 0 ? $"{totalCount} " : "")}scores for {player.name}");

            PlayerScoreCollection psc = GetPlayerScoresPage(1, player, totalCount, true, LastXNumbers(player.scoreStats.totalRankedScore, 9));
            PlayerScore[] func(int page) => GetPlayerScoresPage(page, player, totalCount, true, LastXNumbers(player.scoreStats.totalRankedScore, 9)).playerScores;
            PlayerScore[] playerScores = LoopOverPages(psc.metadata.itemsPerPage, totalCount, 2, func);
            
            DebugPrint(DebugLevel.Dev, $"Finished retriving {totalCount} scores for player {player.name}");
            playerScores = AppendArrays(psc.playerScores, playerScores);
            
            for (int i = 0; i < playerScores.Length; i++)
            {
                PlayerScore score = playerScores[i];

                if (incorrectMaxScoreMaps.ContainsKey(score.leaderboard.id))
                {
                    score.leaderboard.maxScore = incorrectMaxScoreMaps[score.leaderboard.id];
                }
            }
            
            playerScores = UpdatePlayerScores(playerScores);
            
            return playerScores;
        }

        /// <summary>
        /// Gets a given page of scores from the given player.
        /// </summary>
        /// <param name="page">The page of the request.</param>
        /// <param name="playerID">The id of the player to get scores from.</param>
        /// <param name="limit">The number of scores to get in a single request; max is 100.</param>
        /// <param name="sort">What to sort by. (top, recent)</param>
        /// <param name="shouldAttemptLoadData">Whether or not the program should attempt to load any local data on the computer.</param>
        /// <param name="assumedTotalCount">The supposed total number of items in the requested collection; used to detect if data is out of sync/too old. Only usable with collections!</param>
        /// <returns>A page of scores from a given player in the form of a collection.</returns>
        private static PlayerScoreCollection GetPlayerScoresPage(int page, Player player, int limit, bool shouldAttemptLoadData, int assumedTotalCount)
        {
            limit = (int)MathF.Min(limit, 100);
            DebugPrint(DebugLevel.Dev, $"Getting {(limit > 0 ? $"{limit} " : "")}scores for player {player.name} page {page}");

            string url = $"player/{player.id}/scores?page={page}{(limit > 0 ? $"&limit={limit}" : "")}&sort=top";
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);
            
            PlayerScoreCollection psc = DeserializeString<PlayerScoreCollection>(results);
            
            psc.player = player;
            SaveAPIData(url, SerializeToJSON(psc));
            return psc;
        }

        #endregion

    }
}