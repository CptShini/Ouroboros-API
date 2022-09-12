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
        public static int GenerateSnipeTimePlaylists(Player sniper, bool local, int n, bool frontPageSnipe)
        {
            string countryCode = local ? sniper.country : "";
            int rank = local ? sniper.countryRank : sniper.rank;

            int from = (int)(rank / 1.2f);
            int to = (int)(rank * 1.2f);

            if (debugLevel >= DebugLevel.None) Console.WriteLine($"Generating Snipe Time playlists for {sniper.name} {countryCode}{(local ? " " : "")}ranks {from} through {to}");

            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Retriving sniper top plays");
            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1);

            float acc = GetAverageAcc(sniperScores.Take(50).ToArray()) - 1f;

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Retriving players that fit criteria");
            Player[] filteredPlayers = GetFilteredPlayers(countryCode, from, to, acc, true).Where(p => p.scoreStats.rankedPlayCount >= 100).Reverse().ToArray();

            if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Generating Snipe Time playlists for filtered players");

            Dictionary<Player, PlayerScore[]> playerScoreDictionary = new();
            foreach (Player player in filteredPlayers)
            {
                if (player.id == sniper.id) { if (debugLevel >= DebugLevel.Full) Console.WriteLine("Skipping over sniper"); continue; }
                PlayerScore[] plays = GetSnipedPlays(player, sniper, sniperScores, !frontPageSnipe, 50, false);
                
                if (plays.Length > 0) { playerScoreDictionary.Add(player, plays); } else if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("No sniped plays, not adding player");
                if (playerScoreDictionary.Count >= n && n >= 0) break;
            }

            if (playerScoreDictionary.Count <= 0) { if (debugLevel >= DebugLevel.None) Console.WriteLine("No players with sniped maps found within criteria, returning"); return 0; }
            int k = 0;
            int highestRank = playerScoreDictionary.Keys.Max(p => local ? p.countryRank : p.rank);
            foreach (KeyValuePair<Player, PlayerScore[]> playerScorePair in playerScoreDictionary)
            {
                if (GenerateSnipeTimePlaylist(playerScorePair.Key, playerScorePair.Value, local, highestRank, CalculatePPGainForRaw(playerScorePair.Value, sniperScores))) k++;
            }

            if (debugLevel >= DebugLevel.None) Console.WriteLine("Finished generating Snipe Time playlists\n");
            return k;
        }

        /// <summary>
        /// Creates a playlist containing any of the sniped players top 50 plays, that the sniper didn't snipe.
        /// </summary>
        /// <param name="snipedPlayer">The player to be sniped.</param>
        /// <param name="snipedPlays"></param>
        /// <param name="local">Set to true, for local rank to be displayed. False, for global.</param>
        /// <param name="highestRank"></param>
        public static bool GenerateSnipeTimePlaylist(Player snipedPlayer, PlayerScore[] snipedPlays, bool local, int highestRank, float ppGain)
        {
            string name = snipedPlayer.name;
            int rank = local ? snipedPlayer.countryRank : snipedPlayer.rank;

            string title = $"({RankName(rank, highestRank)}/{snipedPlayer.scoreStats.averageRankedAccuracy:00.00}%) {name}{(ppGain == -1f ? "" : $" ({ppGain:0.00}pp)")}";
            string path = @"Sniping\";


            if (debugLevel >= DebugLevel.None) Console.WriteLine("Generating Snipe Time playlist for " + name);

            return GenerateBPList(title, path, ConvertPlayerScoreToLeaderboard(snipedPlays));
        }

        public static bool GenerateSnipeTimePlaylistByID(long sniperID, long snipedID)
        {
            Player sniped = GetPlayerInfoFull(snipedID);
            Player sniper = GetPlayerInfoFull(sniperID);
            
            return GenerateSnipeTimePlaylist(sniped, GetSnipedPlays(sniped, sniper, null, false, -1, true), false, 100000, -1f);
        }

        /// <summary>
        /// Creates a playlist containing the top 50 plays of the given player.
        /// </summary>
        /// <param name="player">The player who's plays to get.</param>
        public static void GenerateTopPlaysPlaylist(Player player)
        {
            string name = player.name;
            string title = name + " Top plays";
            string path = @"#\";

            if (debugLevel >= DebugLevel.Basic) Console.WriteLine($"Generating top plays playlist for {name}\n");

            LeaderboardInfo[] topPlayMaps = ConvertPlayerScoreToLeaderboard(GetPlayerScores(player, -1));

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

            LeaderboardInfo[] leaderboards = GetLeaderboards(stars, 0);
            GenerateBPList(title, path, leaderboards);

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
        public static void GenerateReqPlaylists(Player player, int minStars, int maxStars, bool FCReq, bool PlayedReq, bool GenRelAcc)
        {
            if (debugLevel >= DebugLevel.None) Console.WriteLine("Generating Ouroboros Req playlists");

            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Retriving " + player.name + "'s plays");
            PlayerScore[] scores = GetPlayerScores(player, -1);
            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                StarRange stars = new StarRange(i, i + 1);
                LeaderboardInfo[] maps = GetLeaderboards(stars, 0);
                PlayerScore[] starScores = scores.Where(ps => maps.Any(m => m.id == ps.leaderboard.id)).ToArray();

                if (starScores.Length == 0) continue;
                if (FCReq && !GenerateNonFCPlaylist(starScores, stars)) continue;
                if (PlayedReq && !GenerateNotPlayedPlaylist(starScores, stars)) continue;

                if (debugLevel == DebugLevel.None) Console.WriteLine("Generating Ouroboros Req " + stars.name + " playlist");

                GenerateAbsoluteRankReqPlaylist(starScores, stars);
                GenerateRelativeRankReqPlaylist(starScores, stars);
                GenerateAbsoluteAccReqPlaylist(starScores, stars);
                if (GenRelAcc) GenerateRelativeAccReqPlaylist(starScores, stars); else GenerateHighestPPPlaylist(starScores, stars);
                GenerateOldestPlayedPlaylist(starScores, stars);

                if (debugLevel >= DebugLevel.Advanced) Console.WriteLine("Finished generating Ouroboros Req " + stars.name + " playlist");
            }

            if (debugLevel >= DebugLevel.None) Console.WriteLine("Finished generating Ouroboros Req playlists\n");
        }

        /// <summary>
        /// Creates a playlist of the 10 highest ranked plays within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static void GenerateHighestPPPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderByDescending(ps => ps.score.pp).ToArray();

            int n = NumResolve(-1, playerScores.Length);

            string title = $"{stars.name} Best plays";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine($"Generating Ouroboros Req playlist {title} of length {n}");
            
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores).Take(n).ToArray());
        }

        /// <summary>
        /// Creates a playlist of the 20 oldest played maps within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static void GenerateOldestPlayedPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderBy(ps => ps.score.timeSet).ToArray();

            int n = NumResolve(20, playerScores.Length);

            string title = $"{stars.name} %% oldest";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine($"Generating Ouroboros Req playlist {title} of length {n}");

            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores).Take(n).ToArray());
        }

        /// <summary>
        /// Creates a playlist of all the maps not played within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        /// <returns>True, if the number of maps not played are less then or equal to 20. Otherwise, returns false.</returns>
        public static bool GenerateNotPlayedPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            LeaderboardInfo[] maps = GetLeaderboards(stars, 0);

            maps = maps.Where(m => !playerScores.Any(ps => ps.leaderboard.id == m.id)).ToArray();

            string title = stars.name + " not played";
            if (GenerateBPList(title, @"Øuroboros\", maps))
            {
                if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + maps.Length);
            }
            return maps.Length <= 20;
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
            PlayerScore[] normalMaps = playerScores.Where(ps => (ps.leaderboard.createdDate - DateTime.Parse("01-11-2019")).TotalDays >= 0f).ToArray();
            PlayerScore[] eldertechMaps = playerScores.Where(ps => (ps.leaderboard.createdDate - DateTime.Parse("01-11-2019")).TotalDays < 0f).ToArray();

            PlayerScore[] recentMaps = normalMaps.Where(ps => (DateTime.Now - ps.score.timeSet).TotalDays < 180f).ToArray();
            PlayerScore[] oldMaps = normalMaps.Where(ps => (DateTime.Now - ps.score.timeSet).TotalDays >= 180f).ToArray();
            playerScores = AppendArrays(AppendArrays(oldMaps, recentMaps), eldertechMaps);

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
        public static void GenerateAbsoluteAccReqPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderBy(ps => ps.accuracy).ToArray();

            List<PlayerScore> scores = new();
            float accReq = 75f;
            float lowestAcc = 0f;
            int prevCount = 0;
            for (; scores.Count <= 10 && accReq < 99.9f; accReq += AccReqIncrementResolve(accReq))
            {
                prevCount = scores.Count;
                scores = playerScores.Where(ps => ps.accuracy <= accReq).ToList();
                if (scores.Count > prevCount) lowestAcc = accReq;
            }
            scores = playerScores.Where(ps => ps.accuracy <= accReq).ToList();
            lowestAcc += AccReqIncrementResolve(lowestAcc);

            string title = $"{stars.name} #% <{lowestAcc:00.00}";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
            //GenerateBPList($"{stars.name} Best #%", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderByDescending(ps => ps.accuracy).ToArray()));
        }

        public static void GenerateRelativeAccReqPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = GetRelativeAccuracy(playerScores);
            playerScores = playerScores.OrderBy(ps => ps.relativeAccuracy).ToArray();

            List<PlayerScore> scores = new();
            float accReq = 75f;
            float lowestAcc = 0f;
            int prevCount = 0;
            for (; scores.Count <= 10 && accReq < 99.9f; accReq += AccReqIncrementResolve(accReq))
            {
                prevCount = scores.Count;
                scores = playerScores.Where(ps => ps.relativeAccuracy <= accReq).ToList();
                if (scores.Count > prevCount) lowestAcc = accReq;
            }
            scores = playerScores.Where(ps => ps.relativeAccuracy <= accReq).ToList();
            lowestAcc += AccReqIncrementResolve(lowestAcc);

            string title = $"{stars.name} %% <{lowestAcc:00.00}";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
            //GenerateBPList($"{stars.name} Best %%", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderByDescending(ps => ps.relativeAccuracy).ToArray()));
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

            string title = $"{stars.name} ## >{lowestRank}";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
            //GenerateBPList($"{stars.name} Best ##", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderBy(ps => ps.score.rank).ToArray()));
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst relative rank within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static void GenerateRelativeRankReqPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderByDescending(ps => ps.relativeRank).ToArray();

            List<PlayerScore> scores = new();
            float rankRelReq = 55f;
            float lowestRel = 0f;
            int prevCount = 0;
            for (; scores.Count <= 10 && rankRelReq >= 0.1f; rankRelReq -= RelRankReqDecrementResolve(rankRelReq))
            {
                prevCount = scores.Count;
                scores = playerScores.Where(ps => ps.relativeRank >= rankRelReq).ToList();
                if (scores.Count > prevCount) lowestRel = rankRelReq;
            }
            scores = playerScores.Where(ps => ps.relativeRank >= rankRelReq).ToList();
            lowestRel -= RelRankReqDecrementResolve(lowestRel);

            string title = $"{stars.name} %# >{lowestRel:00.0}";
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Generating Ouroboros Req playlist " + title + " of length " + scores.Count);
            GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(scores.ToArray()));
            //GenerateBPList($"{stars.name} Best %#", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderBy(ps => ps.relativeRank).ToArray()));
        }

        public static PlayerScore[] GetRelativeAccuracy(PlayerScore[] scores)
        {
            for (int i = 0; i < scores.Length; i++)
            {
                Score[] mapScores = GetLeaderboardScores(scores[i].leaderboard.id, (int)MathF.Min(12, scores[i].score.rank), "", "");
                PlayerScore[] playerScores = ConvertScoreToPlayerScore(scores[i].leaderboard, mapScores);
                scores[i].relativeAccuracy = scores[i].accuracy / GetAverageAcc(playerScores) * 100f;
            }

            return scores;
        }

        #endregion

        public static void GenerateSongSuggest(Player player, bool removeAlreadyBeat)
        {
            int rank = player.rank;
            PlayerScore[] userScores = GetPlayerScores(player, -1);
            Player[] players = GetPlayersByRank("", (int)(rank / 1.2f), (int)(rank * 1.2f));

            float targetAccuracy = GetAverageAcc(userScores.Take(50).ToArray()) + 0.5f;

            List<PlayerScore[]> playerScores = new List<PlayerScore[]>();
            for (int i = 0; i < players.Length; i++)
            {
                PlayerScore[] scores = GetPlayerScores(players[i], 50);
                playerScores.Add(scores);
            }

            List<Map> mapList = new List<Map>();
            foreach (PlayerScore[] scores in playerScores)
            {
                for (int i = 0; i < scores.Length; i++)
                {
                    PlayerScore play = scores[i];
                    float d = play.accuracy - targetAccuracy;

                    if (MathF.Abs(d) > 2) continue;

                    float score = (MathF.Pow(50 - i, 1.5f) / 10) * (1 - d * d / 4);

                    LeaderboardInfo lb = play.leaderboard;

                    if (mapList.Exists(map => map.map.id == lb.id))
                    {
                        Map m = mapList.Where(map => map.map.id == lb.id).ToArray()[0];

                        m.count++;
                        m.scores.Add(score);
                    }
                    else
                    {
                        Map m = new Map()
                        {
                            map = lb,
                            count = 1,
                            scores = new List<float>()
                        };
                        m.scores.Add(score);
                        mapList.Add(m);
                    }

                }
            }
            foreach (Map map in mapList)
            {
                for (int i = 0; i < map.scores.Count; i++)
                {
                    map.score += map.scores[i] * MathF.Pow(0.955f, i);
                }

            }

            mapList = mapList.OrderByDescending(m => m.score).ToList();

            LeaderboardInfo[] maps = mapList.Select(map => map.map).ToArray();
            if (removeAlreadyBeat) maps = maps.Where(lb => !userScores.Where(ps => ps.score.fullCombo).Any(ps => ps.leaderboard.id == lb.id)).ToArray();


            GenerateBPList($"Top 100 maps for {player.name} @ {targetAccuracy:00.00}%", @"Øuroboros\", maps.Take(100).ToArray());
            if (debugLevel >= DebugLevel.Basic) Console.WriteLine("Finished generating song suggestion playlist\n");
        }

    }
}