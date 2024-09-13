using OuroborosLibrary.ScoreSaberClasses;
using static OuroborosLibrary.SaveLoad.JSONConverter;
using static OuroborosLibrary.Queries.QueryHelpers.PageHandler;
using static OuroborosLibrary.Queries.QueryHelpers.CustomAPIDataFixer;

namespace OuroborosLibrary.Queries
{
    public static class APIQueries
    {
        #region Player Queries

        /// <summary>
        /// Gets all players within the given rank range.
        /// </summary>
        /// <param name="rankFrom">The best rank to -get; the low number.</param>
        /// <param name="rankTo">The worst rank to -get; the high number.</param>
        /// <param name="countries">The country from which to fetch the players, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>All players within the given rank range.</returns>
        public static Player[] GetPlayers(int rankFrom, int rankTo, string countries = "")
        {
            bool bottomRankOnStartOfPage = rankFrom % 50 == 0;
            int extraPage = bottomRankOnStartOfPage ? 0 : 1;
            int startingPage = rankFrom / 50 + extraPage;

            Collection<Player> pageFunction(int page) => GetPlayersPage(page, countries);
            Player[] players = GetConjoinPages(rankTo, pageFunction, startingPage);

            bool PlayerWithinRankRange(Player player, int rankFrom, int rankTo)
            {
                bool global = countries == "";
                int rank = global ? player.rank : player.countryRank;

                return rankTo >= rank && rankFrom <= rank;
            }

            players = players.Where(p => PlayerWithinRankRange(p, rankFrom, rankTo)).ToArray(); // Trims excess players not requested.

            return players;
        }

        /// <summary>
        /// Gets a desired number of players; going from highest to lowest rank.
        /// </summary>
        /// <param name="desiredAmount">The number of players to get.</param>
        /// <param name="countries">The country from which to fetch the players, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>Top [desiredAmount] players.</returns>
        public static Player[] GetPlayers(int desiredAmount, string countries = "")
        {
            Collection<Player> pageFunction(int page) => GetPlayersPage(page, countries);
            Player[] players = GetConjoinPages(desiredAmount, pageFunction);

            return players;
        }

        /// <summary>
        /// Gets a page of players.
        /// </summary>
        /// <param name="page">The page of players to request from.</param>
        /// <param name="countries">The country from which to fetch the players, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>A collection containing a page of players.</returns>
        internal static Collection<Player> GetPlayersPage(int page = 0, string countries = "")
        {
            string result = BaseAPICalls.GetPlayers(page, countries);

            PlayerCollection pc = DeserializeString<PlayerCollection>(result);

            return new Collection<Player>(pc.players, pc.metadata);
        }

        /// <summary>
        /// Gets the number of active players in the world or from a given country.
        /// </summary>
        /// <param name="countries">The country from which to fetch the player count, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>The number of active players in the world or from the given country if provided.</returns>
        public static int GetPlayerCount(string countries = "")
        {
            string result = BaseAPICalls.GetPlayerCount(countries);

            return DeserializeString<int>(result);
        }

        /// <summary>
        /// Gets the information of a given player.
        /// </summary>
        /// <param name="playerId">The ID of the player whose info is being requested.</param>
        /// <param name="full">Whether or not to include scoreStats.</param>
        /// <returns>The player and their info.</returns>
        public static Player GetPlayerInfo(long playerId, bool full = true)
        {
            string result = full ? BaseAPICalls.GetPlayerInfoFull(playerId) : BaseAPICalls.GetPlayerInfoBasic(playerId);

            return DeserializeString<Player>(result);
        }

        /// <summary>
        /// Gets the scores of a given player.
        /// </summary>
        /// <param name="player">The player from which to get scores.</param>
        /// <param name="amount">The number of desired scores; default is -1 which returns ALL scores.</param>
        /// <returns>An array of scores from the given player.</returns>
        public static PlayerScore[] GetPlayerScores(this Player player, int amount = -1)
        {
            bool noRankedPlays = player.scoreStats.rankedPlayCount <= 0;
            if (noRankedPlays) return Array.Empty<PlayerScore>();

            int totalCount = ItemCountLimiter(amount, player.scoreStats.rankedPlayCount); // Ensures that only ranked plays are gotten.

            Collection<PlayerScore> pageFunction(int page) => GetPlayerScoresPage(player, totalCount, page);
            PlayerScore[] playerScores = GetConjoinPages(totalCount, pageFunction);

            UpdateWithCustomData(playerScores); // Adds accuracy, relative rank, and beatmap name to player scores.

            return playerScores;
        }

        /// <summary>
        /// Gets a page of scores from a given player.
        /// </summary>
        /// <param name="player">The player whose scores are being requested.</param>
        /// <param name="limit">The number of scores to get per page; limited at 100.</param>
        /// <param name="page">The page of scores to get; dependent on 'limit'.</param>
        /// <returns>A collection containing a page scores from the requested player.</returns>
        internal static Collection<PlayerScore> GetPlayerScoresPage(Player player, int limit = 8, int page = 0)
        {
            string result = BaseAPICalls.GetPlayerScores(player.id, Math.Min(limit, 100), page);

            PlayerScoreCollection psc = DeserializeString<PlayerScoreCollection>(result);

            return new Collection<PlayerScore>(psc.playerScores, psc.metadata);
        }

        #endregion

        #region Leaderboard Queries

        /// <summary>
        /// Gets all maps/leaderboards inside the given star range.
        /// </summary>
        /// <param name="minStar">The minimum star difficulty of the requested maps.</param>
        /// <param name="maxStar">The maximum star difficulty of the requested maps.</param>
        /// <returns>All maps in the given star range.</returns>
        public static LeaderboardInfo[] GetLeaderboards(int minStar = 0, int maxStar = 14)
        {
            List<LeaderboardInfo> maps = new();
            for (int currentStar = minStar; currentStar < maxStar; currentStar++) // Split up by star rating to reduce API calls.
            {
                LeaderboardInfo[] lbs = GetLeaderboards(currentStar);
                maps.InsertRange(0, lbs); // Because lbs is sorted by star rating descendingly.
            }

            return maps.ToArray();
        }

        /// <summary>
        /// Gets all maps within the given star difficulty.
        /// </summary>
        /// <param name="stars">The lower number of the star difficulty.</param>
        /// <returns>All leaderboards within the given star difficulty; i.e. stars = 8 | returns maps in 8-9.</returns>
        private static LeaderboardInfo[] GetLeaderboards(int stars)
        {
            Collection<LeaderboardInfo> pageFunction(int page) => GetLeaderboardsPage(page, stars, stars + 1);
            LeaderboardInfo[] lbs = GetConjoinPages(-1, pageFunction);
            
            UpdateWithCustomData(lbs); // Adds beatmap name for all maps in 'lbs'; e.g: "Cheatreal (ExpertPlus)".

            return lbs;
        }

        /// <summary>
        /// Gets a page of maps/leaderboards inside a given star range.
        /// </summary>
        /// <param name="page">The page of maps to get from.</param>
        /// <param name="minStar">The minimum star difficulty of the requested maps.</param>
        /// <param name="maxStar">The maximum star difficulty of the requested maps.</param>
        /// <returns>A collection containing a page of leaderboards/maps.</returns>
        internal static Collection<LeaderboardInfo> GetLeaderboardsPage(int page = 0, int minStar = 0, int maxStar = 14)
        {
            string result = BaseAPICalls.GetLeaderboards(page, minStar, maxStar);

            LeaderboardInfoCollection lbic = DeserializeString<LeaderboardInfoCollection>(result);

            return new Collection<LeaderboardInfo>(lbic.leaderboards, lbic.metadata);
        }

        /// <summary>
        /// Gets information about a map/leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The ID of the map from which to get information.</param>
        /// <returns>A leaderboardInfo containing information about a map such as name, difficulty, number of plays, etc.</returns>
        public static LeaderboardInfo GetLeaderboardInfo(long leaderboardId)
        {
            string result = BaseAPICalls.GetLeaderboardInfo(leaderboardId);

            return DeserializeString<LeaderboardInfo>(result);
        }

        /// <summary>
        /// Gets a desired number of scores on a given map.
        /// </summary>
        /// <param name="leaderboard">The map from which to get scores.</param>
        /// <param name="desiredAmount">The number of scores to get; starting from the top going down.</param>
        /// <param name="countries">The country from which to fetch player scores on the given map, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>An array of scores from the given map.</returns>
        public static Score[] GetLeaderboardScores(this LeaderboardInfo leaderboard, int desiredAmount = 12, string countries = "")
        {
            Collection<Score> pageFunction(int page) => GetLeaderboardScoresPage(leaderboard, page, countries);
            Score[] scores = GetConjoinPages(desiredAmount, pageFunction);

            return scores;
        }

        /// <summary>
        /// Gets a page of scores on a given map.
        /// </summary>
        /// <param name="leaderboard">The map from which to get scores.</param>
        /// <param name="page">The page of scores to get.</param>
        /// <param name="countries">The country from which to fetch player scores on the given map, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>A collection containing a page of scores from the given leaderboard.</returns>
        internal static Collection<Score> GetLeaderboardScoresPage(LeaderboardInfo leaderboard, int page = 0, string countries = "")
        {
            string result = BaseAPICalls.GetLeaderboardScores(leaderboard.id, page, countries);

            ScoreCollection sc = DeserializeString<ScoreCollection>(result);

            return new Collection<Score>(sc.scores, sc.metadata);
        }

        #endregion
    }
}