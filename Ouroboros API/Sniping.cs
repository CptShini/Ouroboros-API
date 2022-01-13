using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using System;
using System.Collections.Generic;

namespace Ouroboros_API
{
    /// <summary>
    /// A library containing all the neccessary functions to manage sniping.
    /// </summary>
    public static class Sniping
    {

        #region Sniping Functions

        /// <summary>
        /// Gets the maps of the sniped players top 50 plays, that the sniper didn't snipe.
        /// </summary>
        /// <param name="snipedPlayer">The player to be sniped.</param>
        /// <param name="sniperPlayer">The player sniping.</param>
        /// <param name="_sniperScores">The scores of the sniper; set to null to make function get them itself.</param>
        /// <returns>Any maps in the sniped players top 50, that wasn't sniped by the sniper.</returns>
        public static LeaderboardInfo[] GetSnipedMaps(Player snipedPlayer, Player sniperPlayer, PlayerScore[] _sniperScores)
        {
            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Retriving maps that " + sniperPlayer.name + " sniped " + snipedPlayer.name + " on");

            PlayerScore[] snipedScores = GetPlayerScores(snipedPlayer, 50, "top").playerScores;
            PlayerScore[] sniperScores = _sniperScores ?? GetPlayerScores(sniperPlayer, -1, "top").playerScores;
            LeaderboardInfo[] snipedMaps = GetSnipedPlays(snipedScores, sniperScores);

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Finished retriving sniped maps");
            return snipedMaps;
        }

        /// <summary>
        /// Filters out any map the sniper has already sniped the sniped player on.
        /// </summary>
        /// <param name="snipedScores">The scores of the player to be sniped.</param>
        /// <param name="sniperScores">The scores of the player sniping.</param>
        /// <returns>Any of the sniped players given scores, the sniper hasn't sniped.</returns>
        private static LeaderboardInfo[] GetSnipedPlays(PlayerScore[] snipedScores, PlayerScore[] sniperScores)
        {
            List<LeaderboardInfo> snipedPlays = new();
            Dictionary<long, PlayerScore> sniperScoresDic = ConvertToIdPlayerScoreDictionary(sniperScores);

            if (debugLevel >= DebugLevel.Full) Console.WriteLine("Filtering out sniped scores");
            foreach (PlayerScore play in snipedScores)
            {
                if (!WasSnipe(play, sniperScoresDic)) snipedPlays.Add(play.leaderboard);
            }

            if (debugLevel >= DebugLevel.Dev) Console.WriteLine("Finished filtering out sniped scores");
            return snipedPlays.ToArray();
        }

        /// <summary>
        /// Checks whether or not sniper has sniped given score.
        /// </summary>
        /// <param name="snipedPlay">The play to be sniped.</param>
        /// <param name="sniperScores">The id-score dictionary of the player sniping.</param>
        /// <returns>True, if sniper has played map AND sniped player. False, if sniper didn't snipe player, or hasn't played map.</returns>
        private static bool WasSnipe(PlayerScore snipedPlay, Dictionary<long, PlayerScore> sniperScores)
        {
            return sniperScores.TryGetValue(snipedPlay.leaderboard.id, out PlayerScore sniperPlay) && sniperPlay.acc > snipedPlay.acc;
        }

        #endregion

    }
}