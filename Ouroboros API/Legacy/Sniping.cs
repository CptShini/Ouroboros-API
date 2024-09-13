using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.Legacy.Core;
using static Ouroboros_API.Legacy.Queries;
using System.Collections.Generic;
using System.Linq;

namespace Ouroboros_API.Legacy
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
        public static PlayerScore[] GetSnipedPlays(Player snipedPlayer, Player sniperPlayer, PlayerScore[] _sniperScores, bool weightByPP, int topN, bool playedByBoth)
        {
            DebugPrint(DebugLevel.Full, $"Retriving maps that {sniperPlayer.name} sniped {snipedPlayer.name} on");

            PlayerScore[] snipedScores = GetPlayerScores(snipedPlayer, topN)/*.Where(ps => ps.score.rank > (int)(snipedPlayer.rank / 100f)).ToArray()*/;
            PlayerScore[] sniperScores = _sniperScores ?? GetPlayerScores(sniperPlayer, -1);
            PlayerScore[] snipedPlays = GetSnipedPlays(snipedScores, sniperScores, weightByPP);

            if (playedByBoth) snipedPlays = snipedPlays.Where(ps => sniperScores.Any(s => s.leaderboard.id == ps.leaderboard.id)).ToArray();

            DebugPrint(DebugLevel.Dev, "Finished retriving sniped maps");
            return snipedPlays;
        }

        /// <summary>
        /// Filters out any map the sniper has already sniped the sniped player on.
        /// </summary>
        /// <param name="snipedScores">The scores of the player to be sniped.</param>
        /// <param name="sniperScores">The scores of the player sniping.</param>
        /// <returns>Any of the sniped players given scores, the sniper hasn't sniped.</returns>
        private static PlayerScore[] GetSnipedPlays(PlayerScore[] snipedScores, PlayerScore[] sniperScores, bool weightByPP)
        {
            List<PlayerScore> snipedPlays = new();
            Dictionary<long, PlayerScore> sniperScoresDic = ConvertToIdPlayerScoreDictionary(sniperScores);

            DebugPrint(DebugLevel.Full, "Filtering out sniped scores");
            foreach (PlayerScore play in snipedScores)
            {
                if (weightByPP && play.score.pp < sniperScores[31].score.pp) break;
                if (!WasSnipe(play, sniperScoresDic)) snipedPlays.Add(play);
            }

            DebugPrint(DebugLevel.Dev, "Finished filtering out sniped scores");

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
            bool bothPlayed = sniperScores.TryGetValue(snipedPlay.leaderboard.id, out PlayerScore sniperPlay);

            if (!bothPlayed) return false;

            bool sniperPlayBetter = sniperPlay.score.baseScore > snipedPlay.score.baseScore || sniperPlay.score.baseScore == sniperPlay.leaderboard.maxScore;

            if (!sniperPlayBetter) return false;

            return true;
        }

        #endregion

    }
}