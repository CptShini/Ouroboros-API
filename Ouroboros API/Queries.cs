using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ouroboros_API
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

        #region Leaderboard Queries

        /// <summary>
        /// Gets all ranked leaderboards within the given star range.
        /// </summary>
        /// <param name="stars">The range of star difficulty the maps must lie in.</param>
        /// <param name="sort">The direction which to sort by star difficulty. (0 = descending, 1 = ascending)</param>
        /// <returns>A collection of all ranked maps within the given star range.</returns>
        public static LeaderboardInfoCollection GetLeaderboards(StarRange stars, int sort)
        {
            int minStar = (int)MathF.Round(stars.minStars, MidpointRounding.ToNegativeInfinity);
            int maxStar = (int)MathF.Round(stars.maxStars, MidpointRounding.ToPositiveInfinity);

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Retriving " + (sort == 0 ? "decending" : "ascending") + " ranked leaderboards between " + minStar + " and " + maxStar + " stars");

            LeaderboardInfoCollection lbic = GetLeaderboardsPage(1, minStar, maxStar, sort, false, -1);
            LeaderboardInfo[] func(int page) => GetLeaderboardsPage(page, minStar, maxStar, sort, true, lbic.metadata.total).leaderboards;
            LeaderboardInfo[] lbs = LoopOverPages(lbic.metadata.itemsPerPage, lbic.metadata.total, 2, func);

            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Finished retriving leaderboards");
            lbic.leaderboards = AppendArrays(lbic.leaderboards, lbs);
            lbic.leaderboards = lbic.leaderboards.Where(m => !glitchedMaps.Any(id => id == m.id)).ToArray();
            lbic.leaderboards.ToList().ForEach(lb => lb.songNameWDiff = lb.songName + " (" + ResolveDifficultyName(lb.difficulty.difficulty) + ")");
            return lbic;
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
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Getting " + (sort == 0 ? "decending" : "ascending") + " ranked leaderboards between " + minStar + " and " + maxStar + " stars page " + page);

            string url = "leaderboards?ranked=true&category=3&page=" + page + (minStar >= 0 ? "&minStar=" + minStar : "") + (maxStar >= 0 ? "&maxStar=" + maxStar : "") + (sort >= 0 ? "&sort=" + sort : "");
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Finished getting leaderboards page " + page);
            return DeserializeString<LeaderboardInfoCollection>(results);
        }

        /// <summary>
        /// Gets the info for a given leaderboard using its ID.
        /// </summary>
        /// <param name="leaderboardID">The id of the leaderboard.</param>
        /// <returns>The leaderboard's info.</returns>
        public static LeaderboardInfo GetLeaderboardInfo(long leaderboardID)
        {
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Getting leaderboard info for " + leaderboardID);

            string url = "leaderboard/by-id/" + leaderboardID + "/info";
            string results = GetContents(url, true, -1);

            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Finished getting leaderboard info for " + leaderboardID);
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
        public static ScoreCollection GetLeaderboardScores(long leaderboardID, int n, string countryCode, string search)
        {
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Retriving " + (countryCode.Length > 0 ? countryCode + " " : "") + "leaderboard scores " + (search.Length > 0 ? " with name " + search + " " : "") + "for " + leaderboardID);

            ScoreCollection sc = GetLeaderboardScoresPage(1, leaderboardID, countryCode, search, false, -1);
            sc.metadata.total = NumResolve(n, sc.metadata.total);
            Score[] func(int page) => GetLeaderboardScoresPage(page, leaderboardID, countryCode, search, true, sc.metadata.total).scores;
            Score[] scores = LoopOverPages(sc.metadata.itemsPerPage, sc.metadata.total, 2, func);

            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Finished retriving " + sc.metadata.total + " leaderboard scores for " + leaderboardID);
            sc.scores = AppendArrays(sc.scores, scores);
            return sc;
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
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Getting " + (countryCode.Length > 0 ? countryCode + " " : "") + "leaderboard scores " + (search.Length > 0 ? " with name " + search + " " : "") + "for " + leaderboardID + " page " + page);

            string url = "leaderboard/by-id/" + leaderboardID + "/scores?page=" + page + (countryCode.Length > 0 ? "&countries=" + countryCode : "") + (search.Length > 0 ? "&search=" + search : "");
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Finished getting leaderboard scores for " + leaderboardID + " page " + page);
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
        public static Player[] GetFilteredPlayers(string countryCode, int rankFrom, int rankTo, float minAcc, bool computeAcc)
        {
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Retriving filtered leaderboards for " + (countryCode.Length > 0 ? countryCode + " " : "") + "ranks " + rankFrom + " through " + rankTo + (minAcc > 0 ? " with min. avg. acc being " + minAcc.ToString("00.00") + "%" : ""));
            Player[] players = GetPlayersByRank(countryCode, rankFrom, rankTo);
            if (computeAcc) players = CalculateAverageAccuracy(players, 50);
            List<Player> filteredPlayers = new();

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Filtering out players that dont fit criteria");
            foreach (Player player in players)
            {
                if (player.scoreStats.averageRankedAccuracy >= minAcc) filteredPlayers.Add(player);
            }

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished filtering out players");
            return filteredPlayers.ToArray();
        }

        /// <summary>
        /// Calculates the average accuracy for an array of players by getting their top n scores.
        /// </summary>
        /// <param name="players">The players who's average accuracy amongst their top n scores, you wish to calculate.</param>
        /// <param name="topN">The number of top scores, from which to compute the average accuracy.</param>
        /// <returns>The input array, but with an updated player.scoreStats.averageRankedAccuracy value.</returns>
        public static Player[] CalculateAverageAccuracy(Player[] players, int topN)
        {
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine($"Calculating average acc by top {topN} scores");

            foreach (Player player in players)
            {
                player.scoreStats.averageRankedAccuracy = GetAverageAcc(GetPlayerScores(player, topN, "top").playerScores);
            }

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished calculating average acc");
            return players;
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
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Retriving " + (countryCode.Length > 0 ? countryCode + " " : "") + "leaderboards for ranks " + rankFrom + " through " + rankTo);

            int startingPage = rankFrom / 50 + 1;
            PlayerCollection pc = GetPlayersPage(startingPage, countryCode, "", false, -1);
            Player[] func(int page) => GetPlayersPage(page, countryCode, "", true, pc.metadata.total).players;
            Player[] players = LoopOverPages(pc.metadata.itemsPerPage, rankTo, startingPage + 1, func);
            players = AppendArrays(pc.players, players).Where(p => rankTo >= (countryCode == "" ? p.rank : p.countryRank) && rankFrom <= (countryCode == "" ? p.rank : p.countryRank)).ToArray();

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished retriving leaderboards by rank");
            return players;
        }

        /// <summary>
        /// Gets top n number of players fitting given criteria.
        /// </summary>
        /// <param name="n">The number of players to get. (pp Top->Bottom)</param>
        /// <param name="countryCode">What countries to filter by using ISO 3166-1 alpha-2 code. (comma delimitered)</param>
        /// <param name="search">The query string to search by for players.</param>
        /// <returns>A collection of players fitting given criteria.</returns>
        public static PlayerCollection GetPlayers(int n, string countryCode, string search)
        {
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Retriving " + n + " players " + (search.Length > 0 ? "with name " + search + " " : "") + (countryCode.Length > 0 ? "from " + countryCode + " " : ""));

            PlayerCollection pc = GetPlayersPage(1, countryCode, search, false, -1);
            pc.metadata.total = NumResolve(n, pc.metadata.total);
            Player[] func(int page) => GetPlayersPage(page, countryCode, search, true, pc.metadata.total).players;
            Player[] players = LoopOverPages(pc.metadata.itemsPerPage, pc.metadata.total, 2, func);

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished retriving " + pc.metadata.total + " players");
            pc.players = AppendArrays(pc.players, players);
            return pc;
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
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Getting players " + (search.Length > 0 ? "with name " + search + " " : "") + (countryCode.Length > 0 ? "from " + countryCode + " " : "") + "page " + page);

            string url = "players?page=" + page + (countryCode.Length > 0 ? "&countries=" + countryCode : "") + (search.Length > 0 ? "&search=" + search : "");
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Finished getting players page " + page);
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
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Getting number of players " + (search.Length > 0 ? "with name " + search + " " : "") + (countryCode.Length > 0 ? "from " + countryCode + " " : ""));

            string url = "players/count?" + (countryCode.Length > 0 ? "&countries=" + countryCode : "") + (search.Length > 0 ? "&search=" + search : "");
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
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Getting basic player info for player " + playerID);

            string url = "player/" + playerID + "/basic";
            string results = GetContents(url, true, -1);

            return DeserializeString<Player>(results);
        }

        /// <summary>
        /// Gets the information of a given player by their ID.
        /// </summary>
        /// <param name="playerID">The ID of the player from whom to get information.</param>
        /// <returns>The information of the given player.</returns>
        public static Player GetPlayerInfoFull(long playerID)
        {
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Getting full player info for player " + playerID);

            string url = "player/" + playerID + "/full";
            string results = GetContents(url, true, -1);

            return DeserializeString<Player>(results);
        }

        /// <summary>
        /// Gets n scores from the given player.
        /// </summary>
        /// <param name="player">The player from which to get scores.</param>
        /// <param name="n">The number of scores to get.</param>
        /// <param name="sort">What to sort by. (top, recent)</param>
        /// <returns>A collection of scores from the given player.</returns>
        public static PlayerScoreCollection GetPlayerScores(Player player, int n, string sort)
        {
            int totalCount = NumResolve(n, player.scoreStats.rankedPlayCount);
            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Retriving " + (sort.Length > 0 ? sort + " " : "") + (totalCount > 0 ? totalCount + " " : "") + "scores for " + player.name);

            PlayerScoreCollection psc = GetPlayerScoresPage(1, player.id, totalCount, sort, false, -1);
            PlayerScore[] func(int page) => GetPlayerScoresPage(page, player.id, totalCount, sort, true, psc.metadata.total).playerScores;
            PlayerScore[] playerScores = LoopOverPages(psc.metadata.itemsPerPage, totalCount, 2, func);

            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Finished retriving " + totalCount + " scores for player " + player.name);
            List<PlayerScore> result = AppendArrays(psc.playerScores, playerScores).ToList();
            result = result.Where(ps => ps.leaderboard.ranked).ToList();
            result.ForEach(ps => ps.acc = (float)ps.score.baseScore / ps.leaderboard.maxScore * 100);
            result.ForEach(ps => ps.val = (float)ps.score.rank / ps.leaderboard.plays * 100);
            result.ForEach(ps => ps.leaderboard.songNameWDiff = ps.leaderboard.songName + " (" + ResolveDifficultyName(ps.leaderboard.difficulty.difficulty) + ")");
            psc.playerScores = result.ToArray();
            return psc;
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
        private static PlayerScoreCollection GetPlayerScoresPage(int page, long playerID, int limit, string sort, bool shouldAttemptLoadData, int assumedTotalCount)
        {
            limit = (int)MathF.Min(limit, 100);
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Getting " + (sort.Length > 0 ? sort + " " : "") + (limit > 0 ? limit + " " : "") + "scores for player " + playerID + " page " + page);

            string url = "player/" + playerID + "/scores?page=" + page + (limit > 0 ? "&limit=" + limit : "") + (sort.Length > 0 ? "&sort=" + sort : "");
            string results = GetContents(url, shouldAttemptLoadData, assumedTotalCount);

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Finished getting scores for player " + playerID + " page " + page);
            return DeserializeString<PlayerScoreCollection>(results);
        }

        #endregion

    }
}