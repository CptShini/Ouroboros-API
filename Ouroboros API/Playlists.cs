using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using static Ouroboros_API.Sniping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ouroboros_API
{
    public static class Playlists
    {

        #region Snipe Time

        /// <summary>
        /// Creates 5 playlists for the player to snipe.
        /// </summary>
        /// <param name="sniper">The player whom will be the sniper.</param>
        /// <param name="minAcc">The minimum average acc, across their top 50 scores, required to be considered a target. (Set to 0 to not -get scores and calculate.)</param>
        /// <param name="local">Set to true, for algorithm to hunt within country. False, for it to hunt globally.</param>
        public static void GenerateSnipeTimePlaylists(Player sniper, float minAcc, bool local)
        {
            string countryCode = local ? sniper.country : "";
            int rank = local ? sniper.countryRank : sniper.rank;

            int from = rank;
            int to = (int)(rank / 50f) * 50 + 50;

            if (debugLevel >= DebugLevel.None) Console.WriteLine($"Generating Snipe Time playlists for {sniper.name} {countryCode}{(local ? " " : "")}ranks {from} through {to}{(minAcc > 0 ? " with min. avg. acc being " + minAcc.ToString("00.00") + "%" : "")}");

            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Retriving sniper top plays");
            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1, "top").playerScores;

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Retriving players that fit criteria");
            Player[] filteredPlayers = GetFilteredPlayers(countryCode, from, to, minAcc, true).Reverse().ToArray();

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Generating Snipe Time playlists for filtered players");
            int i = 0;
            foreach (Player player in filteredPlayers)
            {
                if (player.id == sniper.id) { if (debugLevel >= DebugLevel.Full) Console.WriteLine("Skipping over sniper"); continue; }

                if (GenerateSnipeTimePlaylist(player, sniper, sniperScores)) i++;
                if (i >= 5) break;
            }
            GenerateTopPlaysPlaylist(sniper);

            if (debugLevel >= DebugLevel.None) Console.WriteLine("Finished generating Snipe Time playlists\n");
        }

        /// <summary>
        /// Creates a playlist containing any of the sniped players top 50 plays, that the sniper didn't snipe.
        /// </summary>
        /// <param name="snipedPlayer">The player to be sniped.</param>
        /// <param name="sniperPlayer">The player sniping.</param>
        /// <param name="sniperScores">The scores of the sniper; set to null to make function get them itself.</param>
        /// <returns>True, if playlist generation was successful. False, if playlist wasn't generated.</returns>
        public static bool GenerateSnipeTimePlaylist(Player snipedPlayer, Player sniperPlayer, PlayerScore[] sniperScores)
        {
            string name = snipedPlayer.name;
            string title = name + " Snipe Time";
            string path = @"Sniping\";

            bool playlistDidGen = GenerateBPList(title, path, GetSnipedMaps(snipedPlayer, sniperPlayer, sniperScores));

            if (playlistDidGen && debugLevel >= DebugLevel.None) Console.WriteLine("Generating Snipe Time playlist for " + name);
            return playlistDidGen;
        }

        /// <summary>
        /// Creates a playlist containing the top 50 plays of the given player.
        /// </summary>
        /// <param name="player">The player who's plays to get.</param>
        public static void GenerateTopPlaysPlaylist(Player player)
        {
            string name = player.name;
            string title = name + " Snipe Time";
            string path = @"#\";

            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating top plays playlist for " + name);

            LeaderboardInfo[] topPlayMaps = ConvertPlayerScoreToLeaderboard(GetPlayerScores(player, 50, "top").playerScores);

            GenerateBPList(title, path, topPlayMaps);
        }

        #endregion

        #region Ouroboros

        /// <summary>
        /// Creates several playlists containing all maps between within the given star range.
        /// </summary>
        /// <param name="minStars">The minimum star difficulty.</param>
        /// <param name="maxStars">The maximum star difficulty.</param>
        public static void GenerateOuroboros(int minStars, int maxStars)
        {
            if (debugLevel >= DebugLevel.None) Console.WriteLine("Generating Ouroboros playlists");

            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                GenerateOuroborosPlaylist(new StarRange(i, i + 1));
            }

            if (debugLevel >= DebugLevel.None) Console.WriteLine("Finished generating Ouroboros playlists\n");
        }

        /// <summary>
        /// Creates a playlist with all ranked maps within the given star range.
        /// </summary>
        /// <param name="stars">The range of stars within the maps must lie.</param>
        public static void GenerateOuroborosPlaylist(StarRange stars)
        {
            string path = @"#\";
            string title = stars.name;

            if (debugLevel >= DebugLevel.None) Console.WriteLine("Generating Ouroboros " + title + " playlist");

            LeaderboardInfoCollection leaderboards = GetLeaderboards(stars, 0);
            GenerateBPList(title, path, leaderboards.leaderboards);

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished generating Ouroboros " + title + " playlist");
        }

        #endregion

        #region Ouroboros Req

        /// <summary>
        /// Creates a bunch of playlists divided, into different star ranges, with maps the player needs to improve.
        /// </summary>
        /// <param name="player">The player to do the improving.</param>
        /// <param name="minStars">The minimum star difficulty to improve.</param>
        /// <param name="maxStars">The maximum star difficulty to improve.</param>
        public static void GenerateReqPlaylists(Player player, int minStars, int maxStars)
        {
            if (debugLevel >= DebugLevel.None) Console.WriteLine("Generating Ouroboros Req playlists");

            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Retriving " + player.name + "'s plays");
            PlayerScore[] scores = GetPlayerScores(player, -1, "top").playerScores;
            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                StarRange stars = new StarRange(i, i + 1);
                PlayerScore[] starScores = GetPlayerScoresByStars(scores, stars);
                if (!GenerateNotPlayedPlaylist(starScores, stars)) break;

                if (debugLevel == DebugLevel.None) Console.WriteLine("Generating Ouroboros Req " + stars.name + " playlist");

                if (!GenerateNonFCPlaylist(starScores, stars)) continue;
                GenerateAccReqPlaylist(starScores, stars);
                GenerateAbsoluteRankReqPlaylist(starScores, stars);
                GenerateRelativeRankReqPlaylist(starScores, stars);

                if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished generating Ouroboros Req " + stars.name + " playlist");
            }

            if (debugLevel >= DebugLevel.None) Console.WriteLine("Finished generating Ouroboros Req playlists\n");
        }

        /// <summary>
        /// Creates a playlist of all the maps not played within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        /// <returns>True, if the number of maps not played are less then or equal to 20. Otherwise, returns false.</returns>
        public static bool GenerateNotPlayedPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            LeaderboardInfoCollection maps = GetLeaderboards(stars, 0);

            maps.leaderboards = maps.leaderboards.Where(m => !playerScores.Any(ps => ps.leaderboard.id == m.id)).ToArray();
            maps.metadata.total = maps.leaderboards.Length;

            string title = stars.name + " not played";
            if (GenerateBPList(title, @"Øuroboros\", maps.leaderboards))
            {
                if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + maps.leaderboards.Length);
            }
            return maps.leaderboards.Length <= 20;
        }

        /// <summary>
        /// Creates a playlist of all the maps not FC'ed within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        /// <returns>True, if there are no non-FC'ed maps. Otherwise, returns false.</returns>
        public static bool GenerateNonFCPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.Where(ps => !ps.score.fullCombo).ToArray();

            string title = stars.name + " non FC";
            if (GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.ToArray())))
            {
                if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + playerScores.Length);
            }

            return playerScores.Length == 0;
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst accuracy within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static void GenerateAccReqPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderBy(ps => ps.acc).ToArray();

            List<PlayerScore> scores = new();
            float accReq = 75f;
            float lowestAcc = 0f;
            int prevCount = 0;
            for (; scores.Count <= 10 && accReq < 99.9f; accReq += AccReqIncrementResolve(accReq))
            {
                prevCount = scores.Count;
                scores = playerScores.Where(ps => ps.acc <= accReq).ToList();
                if (scores.Count > prevCount) lowestAcc = accReq;
            }
            scores = playerScores.Where(ps => ps.acc <= accReq).ToList();
            lowestAcc += AccReqIncrementResolve(lowestAcc);

            string title = stars.name + " <" + lowestAcc.ToString("00.00") + "%";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst rank within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static void GenerateAbsoluteRankReqPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderByDescending(ps => ps.score.rank).ToArray();

            List<PlayerScore> scores = new();
            int rankReq = 3000;
            int lowestRank = 0;
            int prevCount = 0;
            for (; scores.Count <= 10 && rankReq > 1; rankReq -= RankReqDecrementResolve(rankReq))
            {
                prevCount = scores.Count;
                scores = playerScores.Where(ps => ps.score.rank >= rankReq).ToList();
                if (scores.Count > prevCount) lowestRank = rankReq;
            }
            scores = playerScores.Where(ps => ps.score.rank >= rankReq).ToList();
            lowestRank -= RankReqDecrementResolve(lowestRank);

            string title = stars.name + " >" + lowestRank + "#";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst relative rank within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static void GenerateRelativeRankReqPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderByDescending(ps => ps.val).ToArray();

            List<PlayerScore> scores = new();
            float rankRelReq = 55f;
            float lowestRel = 0f;
            int prevCount = 0;
            for (; scores.Count <= 10 && rankRelReq >= 0.1f; rankRelReq -= RelRankReqDecrementResolve(rankRelReq))
            {
                prevCount = scores.Count;
                scores = playerScores.Where(ps => ps.val >= rankRelReq).ToList();
                if (scores.Count > prevCount) lowestRel = rankRelReq;
            }
            scores = playerScores.Where(ps => ps.val >= rankRelReq).ToList();
            lowestRel -= RelRankReqDecrementResolve(lowestRel);

            string title = stars.name + " RelRank >" + lowestRel.ToString("00.0") + "%";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
        }

        #endregion

    }
}