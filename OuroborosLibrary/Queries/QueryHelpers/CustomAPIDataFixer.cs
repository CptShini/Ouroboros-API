using OuroborosLibrary.ScoreSaberClasses;

namespace OuroborosLibrary.Queries.QueryHelpers
{
    internal static class CustomAPIDataFixer
    {
        /// <summary>
        /// Updates all inputted maps' beatmap name.
        /// </summary>
        /// <param name="maps">The maps to update.</param>
        internal static void UpdateWithCustomData(IEnumerable<LeaderboardInfo> maps)
        {
            foreach (LeaderboardInfo map in maps)
            {
                map.UpdateMapInfo();
            }
        }

        /// <summary>
        /// Updates all inputted playerscores' accuracy, relative rank, and beatmap name; also filters out non-ranked maps.
        /// </summary>
        /// <param name="playerScores">The playerscores to update.</param>
        internal static void UpdateWithCustomData(IEnumerable<PlayerScore> playerScores)
        {
            playerScores = playerScores.Where(ps => ps.leaderboard.ranked);

            foreach (PlayerScore playerScore in playerScores)
            {
                playerScore.UpdatePlayInfo();
                playerScore.leaderboard.UpdateMapInfo();
            }
        }
    }
}