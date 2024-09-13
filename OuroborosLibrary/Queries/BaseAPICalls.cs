using static OuroborosLibrary.API.APIDataManager;
using static OuroborosLibrary.Queries.QueryHelpers.QueryStringBuilder;

namespace OuroborosLibrary.Queries
{
    public static class BaseAPICalls
    {
        #region Player Calls

        /// <summary>
        /// Gets the full player info for players at the given page globally or from a given country.
        /// </summary>
        /// <param name="page">The page of players to request from.</param>
        /// <param name="countries">The country from which to fetch the players, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>A string containing an array of players and their info.</returns>
        public static string GetPlayers(int page = 0, string countries = "")
        {
            string pageString = GetQueryString("page", page, 0);
            string countriesString = GetQueryString("countries", countries, "");

            string query = $"players?{pageString}{countriesString}";
            string response = GetAPIResponse(query);

            return response;
        }

        /// <summary>
        /// Gets the number of active players in the world or from a given country.
        /// </summary>
        /// <param name="countries">The country from which to fetch the player count, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>A string containing the number of active players in the world or from the given country if provided.</returns>
        public static string GetPlayerCount(string countries = "")
        {
            string countriesString = GetQueryString("countries", countries, "");

            string query = $"players/count?{countriesString}";
            string response = GetAPIResponse(query);

            return response;
        }

        /// <summary>
        /// Gets information about a player.
        /// </summary>
        /// <param name="playerId">The ID of the player whose info is being requested.</param>
        /// <returns>A string containing most of the information about a player; NOT including scoreStats.</returns>
        public static string GetPlayerInfoBasic(long playerId)
        {
            string query = $"player/{playerId}/basic";
            string response = GetAPIResponse(query);

            return response;
        }

        /// <summary>
        /// Gets information about a player.
        /// </summary>
        /// <param name="playerId">The ID of the player whose info is being requested.</param>
        /// <returns>A string containing most of the information about a player; including scoreStats.</returns>
        public static string GetPlayerInfoFull(long playerId)
        {
            string query = $"player/{playerId}/full";
            string response = GetAPIResponse(query);

            return response;
        }

        /// <summary>
        /// Gets the scores of a given player.
        /// </summary>
        /// <param name="playerId">The ID of the player whose scores are being requested.</param>
        /// <param name="limit">The number of scores to get per page; WARNING: DOES NOT EXCEED 100, WILL RETURN AN ERROR.</param>
        /// <param name="page">The page of scores to get; dependent on 'limit'.</param>
        /// <returns>A string containing an array of scores from the requested player.</returns>
        public static string GetPlayerScores(long playerId, int limit = 8, int page = 0)
        {
            string limitString = GetQueryString("limit", limit, 8);
            string pageString = GetQueryString("page", page, 0);

            string query = $"player/{playerId}/scores?{limitString}{pageString}";
            string response = GetAPIResponse(query);

            return response;
        }

        #endregion

        #region Leaderboard Calls

        /// <summary>
        /// Gets maps/leaderboards inside the a given star range.
        /// </summary>
        /// <param name="page">The page of maps to get from.</param>
        /// <param name="minStar">The minimum star difficulty of the requested maps.</param>
        /// <param name="maxStar">The maximum star difficulty of the requested maps.</param>
        /// <returns>A string containing an array of leaderboards, and their info.</returns>
        public static string GetLeaderboards(int page = 0, int minStar = 0, int maxStar = 14)
        {
            string minStarString = GetQueryString("minStar", minStar, 0);
            string maxStarString = GetQueryString("maxStar", maxStar, 14);
            string pageString = GetQueryString("page", page, 0);

            string query = $"leaderboards?ranked=true&category=3{minStarString}{maxStarString}{pageString}"; // Always gets ranked maps sorted by difficulty.
            string response = GetAPIResponse(query);

            return response;
        }

        /// <summary>
        /// Gets information about a map/leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The ID of the map from which to get information.</param>
        /// <returns>A string containing information about a map such as name, difficulty, number of plays, etc.</returns>
        public static string GetLeaderboardInfo(long leaderboardId)
        {
            string query = $"leaderboard/by-id/{leaderboardId}/info";
            string response = GetAPIResponse(query);

            return response;
        }

        /// <summary>
        /// Gets the scores on a given map.
        /// </summary>
        /// <param name="leaderboardId">The ID of the map from which to get scores.</param>
        /// <param name="page">The page of scores to get.</param>
        /// <param name="countries">The country from which to fetch player scores on the given map, where default is global; filtered by ISO 3166-1 alpha-2 code (comma delimitered).</param>
        /// <returns>A string containing an array of scores from the given leaderboard.</returns>
        public static string GetLeaderboardScores(long leaderboardId, int page = 0, string countries = "")
        {
            string countriesString = GetQueryString("countries", countries, "");
            string pageString = GetQueryString("page", page, 0);

            string query = $"leaderboard/by-id/{leaderboardId}/scores?{countriesString}{pageString}";
            string response = GetAPIResponse(query);

            return response;
        }

        #endregion
    }
}