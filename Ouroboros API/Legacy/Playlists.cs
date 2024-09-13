using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.Legacy.DebugManager;
using static Ouroboros_API.Legacy.Core;
using static Ouroboros_API.Legacy.Queries;
using static Ouroboros_API.Legacy.Sniping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

namespace Ouroboros_API.Legacy
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
            int rank = GetRank(sniper, local);

            int from = (int)(rank / 1.2f);
            int to = (int)(rank * 1.2f);

            DebugPrint(DebugLevel.None, $"Generating Snipe Time playlists for {sniper.name} {countryCode}{(local ? " " : "")}ranks {from} through {to}");

            DebugPrint(DebugLevel.Basic, "Retriving sniper top plays");
            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1);

            float acc = GetAverageAcc(sniperScores.Take(50).ToArray()) - 0.5f;

            DebugPrint(DebugLevel.Advanced, "Retriving players that fit criteria");
            Player[] filteredPlayers = GetFilteredPlayers(countryCode, from, to, acc, acc + 1f, true, n).Where(p => p.scoreStats.rankedPlayCount >= 100).Reverse().ToArray();

            DebugPrint(DebugLevel.Advanced, "Generating Snipe Time playlists for filtered players");

            Dictionary<Player, PlayerScore[]> playerScoreDictionary = new();
            foreach (Player player in filteredPlayers)
            {
                if (player.id == sniper.id) { DebugPrint(DebugLevel.Full, "Skipping over sniper"); continue; }
                PlayerScore[] plays = GetSnipedPlays(player, sniper, sniperScores, !frontPageSnipe, 50, false);
                
                if (plays.Length > 0) { playerScoreDictionary.Add(player, plays); } else DebugPrint(DebugLevel.Advanced, "No sniped plays, not adding player");
                if (playerScoreDictionary.Count >= n && n > 0) break;
            }

            if (playerScoreDictionary.Count <= 0) { DebugPrint(DebugLevel.None, "No players with sniped maps found within criteria, returning"); return 0; }
            int k = 0;
            int highestRank = playerScoreDictionary.Keys.Max(p => local ? p.countryRank : p.rank);
            foreach (KeyValuePair<Player, PlayerScore[]> playerScorePair in playerScoreDictionary)
            {
                if (GenerateSnipeTimePlaylist(playerScorePair.Key, playerScorePair.Value, local, highestRank, CalculatePPGainForRaw(playerScorePair.Value, sniperScores))) k++;
            }

            DebugPrint(DebugLevel.None, "Finished generating Snipe Time playlists\n");
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

            string title = $"(#{AddZeros(rank, highestRank)}/{snipedPlayer.scoreStats.averageRankedAccuracy:00.00}%) {name}{(ppGain == -1f ? "" : $" ({ppGain:0.00}pp)")}";
            string path = @"Sniping\";


            DebugPrint(DebugLevel.None, $"Generating Snipe Time playlist for {name}");

            return GenerateBPList(title, path, ConvertPlayerScoreToLeaderboard(snipedPlays));
        }

        public static int GenerateSnipeTargetsPlaylists(Player sniper)
        {
            if (Core.Config.SnipeList.Length == 0) return 0;

            DebugPrint(DebugLevel.None, $"Generating {Core.Config.SnipeList.Length} snipe playlists for {Core.Config.PlayerName}");

            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1);

            List<Player> snipeTargets = new List<Player>();
            Core.Config.SnipeList.ToList().ForEach(id => snipeTargets.Add(GetPlayerInfoFull(id)));

            Dictionary<Player, PlayerScore[]> snipeTargetsDic = new Dictionary<Player, PlayerScore[]>();

            int n = 0;
            for (int i = 0; i < snipeTargets.Count; i++)
            {
                Player sniped = snipeTargets[i];
                sniped.scoreStats.averageRankedAccuracy = GetAverageAcc(GetPlayerScores(sniped, -1).Take(50).ToArray());

                PlayerScore[] snipedScores = GetSnipedPlays(sniped, sniper, sniperScores, false, -1, true);
                if (snipedScores.Length > 0) snipeTargetsDic.Add(sniped, snipedScores);
            }

            int highestPP = (int)snipeTargetsDic.Keys.Select(p => p.pp).Max();
            int highestRank = snipeTargetsDic.Keys.Select(p => p.rank).Max();
            for (int i = 0; i < snipeTargetsDic.Count; i++)
            {
                Player sniped = snipeTargetsDic.Keys.ElementAt(i);
                DebugPrint(DebugLevel.Basic, $"Generating Snipe Time playlist for {sniped.name}");

                string stats = $"({AddZeros((int)sniped.pp, highestPP)}pp | (#{GetRank(sniped)}/#{GetRank(sniped, true)}) @ {sniped.scoreStats.averageRankedAccuracy:00.00}%)";
                string identifier = $"{sniped.name}";
                string ppGain = $"({CalculatePPGainForRaw(snipeTargetsDic[sniped], sniperScores):0.00}pp)";

                string title = $"X {stats} {identifier} {ppGain}";

                if (GenerateBPList(title, @"Sniping\", ConvertPlayerScoreToLeaderboard(snipeTargetsDic[sniped]))) n++;
            }
            Println();

            return n;
        }

        public static bool GenerateSnipeTimePlaylistByID(long sniperID, long snipedID)
        {
            Player sniped = GetPlayerInfoFull(snipedID);
            Player sniper = GetPlayerInfoFull(sniperID);

            PlayerScore[] snipedScores = GetPlayerScores(sniped, -1);
            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1);

            sniped.scoreStats.averageRankedAccuracy = GetAverageAcc(snipedScores.Take(50).ToArray());

            string title = $"Ø ({sniped.rank}/{sniped.scoreStats.averageRankedAccuracy:00.00}%) {sniped.name}";
            string path = @"Sniping\";


            DebugPrint(DebugLevel.None, $"Generating Snipe Time playlist for {sniped.name}");

            return GenerateBPList(title, path, ConvertPlayerScoreToLeaderboard(GetSnipedPlays(sniped, sniper, sniperScores, false, -1, true)));
        }

        /// <summary>
        /// Creates a playlist containing the top 50 plays of the given player.
        /// </summary>
        /// <param name="player">The player whose plays to get.</param>
        public static void GenerateTopPlaysPlaylist(Player player)
        {
            string name = player.name;

            DebugPrint(DebugLevel.None, $"Generating top plays playlist for {name}\n");

            PlayerScore[] scores = GetPlayerScores(player, -1);
            LeaderboardInfo[] topPlayMaps = ConvertPlayerScoreToLeaderboard(scores);

            float avgAccuracy = GetAverageAcc(scores.Take(50).ToArray());

            string title = $"{name} @ {avgAccuracy:00.00}% Top plays";
            string path = @"#\";

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
            DebugPrint(DebugLevel.None, "Generating Ouroboros playlists");

            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                GenerateOuroborosPlaylist(new StarRange(i, i + 1));
            }

            DebugPrint(DebugLevel.None, "Finished generating Ouroboros playlists\n");
        }

        /// <summary>
        /// Creates a playlist with all ranked maps within the given star range.
        /// </summary>
        /// <param name="stars">The range of stars within the maps must lie.</param>
        public static void GenerateOuroborosPlaylist(StarRange stars)
        {
            string path = @"#\";
            string title = stars.name;

            DebugPrint(DebugLevel.None, $"Generating Ouroboros {title} playlist");

            LeaderboardInfo[] leaderboards = GetLeaderboards(stars, 0);
            GenerateBPList(title, path, leaderboards);

            DebugPrint(DebugLevel.Advanced, $"Finished generating Ouroboros {title} playlist");
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
            DebugPrint(DebugLevel.None, "Generating Ouroboros Req playlists");

            DebugPrint(DebugLevel.Basic, $"Retriving {player.name}'s plays");
            PlayerScore[] scores = GetPlayerScores(player, -1);
            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                int n = 0;
                StarRange stars = new StarRange(i, i + 1);
                LeaderboardInfo[] maps = GetLeaderboards(stars, 0);
                PlayerScore[] starScores = scores.Where(ps => maps.Any(m => m.id == ps.leaderboard.id)).ToArray();

                if (starScores.Length == 0) continue;

                if (Core.Config.FcReq)
                {
                    if (!GenerateNonFCPlaylist(starScores, stars))
                    {
                        n++;
                        GenerateEmptyPlaylists(n, $"{stars.name}", @"Øuroboros\");
                        continue;
                    }
                }
                else if (!GenerateNonFCPlaylist(starScores, stars)) n++;

                if (Core.Config.PlayedReq)
                {
                    if (!GenerateNotPlayedPlaylist(starScores, stars))
                    {
                        n++;
                        GenerateEmptyPlaylists(n, $"{stars.name}", @"Øuroboros\");
                        continue;
                    }
                }
                else if (!GenerateNotPlayedPlaylist(starScores, stars)) n++;

                DebugPrint(DebugLevel.None, $"Generating Ouroboros Req {stars.name} playlist", true);

                PlayerScore[] FCScores = starScores.Where(ps => ps.score.fullCombo).ToArray();
                if (!(Core.Config.ReqPlaylistsOnlyFCs && FCScores.Length <= 0))
                {
                    if (GenerateHighestPPPlaylist(starScores, stars)) n++;
                    if (Core.Config.ReqPlaylistsOnlyFCs) starScores = FCScores;

                    if (GenerateAbsoluteRankReqPlaylist(starScores, stars)) n++;
                    if (GenerateAbsoluteAccReqPlaylist(starScores, stars)) n++;
                    if (GenerateRelativeRankReqPlaylist(starScores, stars)) n++;
                    if (GenerateRelativeAccReqPlaylist(starScores, stars)) n++;
                }
                

                GenerateEmptyPlaylists(n, $"{stars.name}", @"Øuroboros\");
                DebugPrint(DebugLevel.Advanced, $"Finished generating Ouroboros Req {stars.name} playlist");
            }

            DebugPrint(DebugLevel.None, "Finished generating Ouroboros Req playlists\n");
        }

        /// <summary>
        /// Creates a playlist of the 10 highest ranked plays within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static bool GenerateHighestPPPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderByDescending(ps => ps.score.pp).ToArray();

            int n = NumResolve(-1, playerScores.Length);
            
            float contributedPP = playerScores.Sum(ps => ps.score.pp * ps.score.weight);

            string title = $"{stars.name} Best plays ({contributedPP:0}pp)";
            DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {n}");
            
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores));
        }

        /// <summary>
        /// Creates a playlist of the 20 oldest played maps within the given star range.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static bool GenerateOldestPlayedPlaylist(PlayerScore[] playerScores, StarRange stars)
        {
            playerScores = playerScores.OrderBy(ps => ps.score.timeSet).ToArray();

            int n = NumResolve(20, playerScores.Length);

            string title = $"{stars.name} %% oldest";
            DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {n}");

            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores).Take(n).ToArray());
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

            maps = maps.Where(m => !playerScores.Any(ps => ps.leaderboard.id == m.id)).Reverse().ToArray();

            if (Core.Config.SplitByElderTech) maps = SplitByEldertech(maps);

            string title = $"{stars.name} not played";
            if (GenerateBPList(title, @"Øuroboros\", maps))
            {
                DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {maps.Length}");
            }
            return maps.Length == 0;
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

            playerScores = SplitPlays(playerScores);

            string title = $"{stars.name} non FC";
            if (GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.ToArray())))
            {
                DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {playerScores.Length}");
            }

            return playerScores.Length == 0;
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst accuracy within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static bool GenerateAbsoluteAccReqPlaylist(PlayerScore[] playerScores, StarRange stars)
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
            DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {scores.Count}");
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(SplitPlaysByEldertech(scores.ToArray())));
            //GenerateBPList($"{stars.name} Best #%", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderByDescending(ps => ps.accuracy).ToArray()));
        }

        public static bool GenerateRelativeAccReqPlaylist(PlayerScore[] playerScores, StarRange stars)
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
            DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {scores.Count}");
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(SplitPlays(scores.ToArray())));
            //GenerateBPList($"{stars.name} Best %%", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderByDescending(ps => ps.relativeAccuracy).ToArray()));
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst rank within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static bool GenerateAbsoluteRankReqPlaylist(PlayerScore[] playerScores, StarRange stars)
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
            DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {scores.Count}");
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(SplitPlays(scores.ToArray())));
            //GenerateBPList($"{stars.name} Best ##", @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores.OrderBy(ps => ps.score.rank).ToArray()));
        }

        /// <summary>
        /// Creates a playlist of the plays with the worst relative rank within the star range using an algorithm to divide plays into stepping stones.
        /// </summary>
        /// <param name="playerScores">All the scores of the player.</param>
        /// <param name="stars">The star range to check within.</param>
        public static bool GenerateRelativeRankReqPlaylist(PlayerScore[] playerScores, StarRange stars)
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
            DebugPrint(DebugLevel.Basic, $"Generating Ouroboros Req playlist {title} of length {scores.Count}");
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(SplitPlays(scores.ToArray())));
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

        public static float GetAverageAccOnMap(LeaderboardInfo leaderboard)
        {
            Score[] mapScores = GetLeaderboardScores(leaderboard.id, 12, "", "");
            PlayerScore[] playerScores = ConvertScoreToPlayerScore(leaderboard, mapScores);
            return GetAverageAcc(playerScores);
        }

        #endregion

        public static int GenerateDominancePlaylist(Player player)
        {
            DebugPrint(DebugLevel.None, $"\nGenerating dominance playlists in {player.country}");

            PlayerScore[] sniperScores = GetPlayerScores(player, -1);
            List<Player> snipeTargets = new List<Player>(GetPlayersByRank(player.country, player.countryRank + 1, -1).Reverse());

            Dictionary<Player, PlayerScore[]> snipeTargetsDic = new Dictionary<Player, PlayerScore[]>();
            foreach (Player sniped in snipeTargets)
            {
                sniped.scoreStats.averageRankedAccuracy = GetAverageAcc(GetPlayerScores(sniped, -1).Take(50).ToArray());

                PlayerScore[] snipedScores = GetSnipedPlays(sniped, player, sniperScores, false, -1, true);
                if (snipedScores.Length <= 0) continue;

                foreach (PlayerScore snipedScore in snipedScores)
                {
                    PlayerScore sniperScore = sniperScores.First(ss => ss.leaderboard.id == snipedScore.leaderboard.id);
                    snipedScore.relativeAccuracy = snipedScore.score.pp - sniperScore.score.pp;
                    snipedScore.score.timeSet = sniperScore.score.timeSet;
                }

                snipeTargetsDic.Add(sniped, SplitPlaysByAge(snipedScores.OrderBy(ps => ps.relativeAccuracy).ToArray()));
            }

            int n = 0;

            int highestPP = (int)snipeTargetsDic.Keys.Select(p => p.pp).Max();
            int highestRank = snipeTargetsDic.Keys.Select(p => p.rank).Max();
            foreach (Player sniped in snipeTargetsDic.Keys)
            {
                DebugPrint(DebugLevel.Basic, $"Generating Snipe Time playlist for {sniped.name}");

                string stats = $"({AddZeros((int)sniped.pp, highestPP)}pp | (#{GetRank(sniped)}/#{GetRank(sniped, true)}) @ {sniped.scoreStats.averageRankedAccuracy:00.00}%)";
                string identifier = $"{sniped.name}";
                string ppGain = $"({CalculatePPGainForRaw(snipeTargetsDic[sniped], sniperScores):0.00}pp)";

                string title = $"Ø {stats} {identifier} {ppGain}";

                if (GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(snipeTargetsDic[sniped]))) n++;
            }
            Println();

            DebugPrint(DebugLevel.Advanced, $"Finished generating dominance playlist in {player.country}");

            return n;
        }

        /*public static void GenerateDominancePlaylist(Player player)
        {
            string countryCode = player.country;

            DebugPrint(DebugLevel.None, $"Generating dominance playlist in {countryCode}");

            List<PlayerScore> playerScores = new List<PlayerScore>();
            PlayerScore[] sniperScores = GetPlayerScores(player, -1);
            
            Player[] players = GetPlayersByRank(countryCode, 1, -1).Reverse().ToArray();

            DebugPrint(DebugLevel.Basic, $"Gettings all ranked scores for {players.Length} players in {countryCode}");

            int prevCount = 0;
            int minRank = 3000;
            for (int i = 0; playerScores.Count < 20; i++)
            {
                prevCount = playerScores.Count;

                Player sniped = players[i];
                PlayerScore[] notSnipedScores = GetSnipedPlays(sniped, player, sniperScores, false, -1, true);
                for (int j = 0; j < notSnipedScores.Length; j++)
                {
                    PlayerScore snipedScore = notSnipedScores[j];
                    PlayerScore sniperScore = sniperScores.Where(ss => ss.leaderboard.id == snipedScore.leaderboard.id).First();
                    sniperScore.relativeAccuracy = snipedScore.score.pp - sniperScore.score.pp;

                    PlayerScore listScore = playerScores.Find(ps => ps.leaderboard.id == snipedScore.leaderboard.id);
                    if (listScore != null)
                    {
                        if (snipedScore.accuracy > listScore.accuracy)
                        {
                            playerScores.Remove(listScore);
                            playerScores.Add(sniperScore);
                        }
                    }
                    else playerScores.Add(sniperScore);
                }

                if (playerScores.Count > prevCount) minRank = sniped.countryRank;
            }

            string title = $"Dominance in {countryCode} @ #{minRank}";

            playerScores = playerScores.OrderBy(ps => ps.relativeAccuracy).ToList();
            LeaderboardInfo[] maps = ConvertPlayerScoreToLeaderboard(SplitPlaysByAge(playerScores.ToArray()));
            GenerateBPList(title, @"Øuroboros\", maps);

            DebugPrint(DebugLevel.Advanced, $"Finished generating dominance playlist in {countryCode}");
        }*/

        public static void GeneratePlayGraphs()
        {
            DebugPrint(DebugLevel.None, "Creating some awesome play-graphs");
            GeneratePlayGraphsForPlayer(Core.Config.PlayerId);
            foreach (long playerId in Core.Config.SnipeList)
            {
                GeneratePlayGraphsForPlayer(playerId);
            }
        }

        private static void GeneratePlayGraphsForPlayer(long id)
        {
            Player player = GetPlayerInfoFull(id);
            DebugPrint(DebugLevel.Basic, $"Creating play-graphs for {player.name}");
            
            SaveAccPlayGraphBMP(player);
            SavePPPlayGraphBMP(player);
            SaveWeightPlayGraphBMP(player);
        }

        private static void SaveAccPlayGraphBMP(Player player)
        {
            PlayerScore[] scores = GetPlayerScores(player, -1).Where(s => s.accuracy >= 80f).ToArray();
            Bitmap bmp = new Bitmap(1000, 500);

            for (int i = 16; i < 20; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    bmp.SetPixel(j, (int)Remap(i, 16, 20, bmp.Height - 1, 0), Color.Gray);
                }
            }

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    bmp.SetPixel((int)Remap(i, 0, 13, 0, bmp.Width - 1), j, Color.Gray);
                }
            }

            for (int i = 0; i < scores.Length; i++)
            {
                float acc = scores[i].accuracy;
                float stars = scores[i].leaderboard.stars;

                int x, y;
                x = (int)Remap(stars, 0, 13, 0, bmp.Width - 1);
                y = (int)Remap(acc, 80, 100, bmp.Height - 1, 0);

                int r = (int)Remap((DateTime.Now - scores[i].score.timeSet).Days, 0, 365, 255, 127);
                if (r < 0) r = 0;
                Color c = Color.FromArgb(r, 0, 0);

                if (i <= 10) c = Color.Cyan;
                else if (i <= 24) c = Color.Green;
                else if (i <= 50) c = Color.Orange;

                bmp.SetPixel(x, y, c);
            }

            /*for (int i = 0; i < bmp.Width; i++)
            {
                float starVal = Remap(i, 0f, bmp.Width, 0f, 13f);

                float avgAcc = GetAverageAccAtStars(starVal, scores);
                avgAcc = GetAverageAccAtStars(starVal, scores.Where(ps => ps.accuracy >= avgAcc).ToArray());
                avgAcc = GetAverageAccAtStars(starVal, scores.Where(ps => ps.accuracy >= avgAcc).ToArray());
                
                

                int y = (int)Remap(avgAcc, 80f, 100f, bmp.Height - 1, 0);
                if (y >= bmp.Height || y < 0) continue;
                bmp.SetPixel(i, y, Color.White);
            }*/

            string path = @$"{$@"{userDataPath}Playgraphs\"}{CleanFileName(player.name).TrimEnd()}\";
            Directory.CreateDirectory(path);
            bmp.Save($"{path}{player.scoreStats.totalRankedScore}acc.png");
            DebugPrint(DebugLevel.Advanced, $"Finished creating playgraph for {player.name}");
        }

        private static void SavePPPlayGraphBMP(Player player)
        {
            PlayerScore[] scores = GetPlayerScores(player, -1);
            Bitmap bmp = new Bitmap(1000, 500);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    bmp.SetPixel(j, (int)Remap(i, 0, 8, bmp.Height - 1, 0), Color.Gray);
                }
            }

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    bmp.SetPixel((int)Remap(i, 0, 13, 0, bmp.Width - 1), j, Color.Gray);
                }
            }

            for (int i = 0; i < scores.Length; i++)
            {
                float pp = scores[i].score.pp;
                float stars = scores[i].leaderboard.stars;

                int x, y;
                x = (int)Remap(stars, 0, 13, 0, bmp.Width - 1);
                y = (int)Remap(pp, 0, 800, bmp.Height - 1, 0);

                int r = (int)Remap((DateTime.Now - scores[i].score.timeSet).Days, 0, 365, 255, 127);
                if (r < 0) r = 0;
                Color c = Color.FromArgb(r, 0, 0);

                if (i <= 10) c = Color.Cyan;
                else if (i <= 24) c = Color.Green;
                else if (i <= 50) c = Color.Orange;

                bmp.SetPixel(x, y, c);
            }

            string path = @$"{$@"{userDataPath}Playgraphs\"}{CleanFileName(player.name).TrimEnd()}\";
            Directory.CreateDirectory(path);
            bmp.Save($"{path}{player.scoreStats.totalRankedScore}pp.png");
            DebugPrint(DebugLevel.Advanced, $"Finished creating pp playgraph for {player.name}");
        }

        private static void SaveWeightPlayGraphBMP(Player player)
        {
            PlayerScore[] scores = GetPlayerScores(player, -1);
            Bitmap bmp = new Bitmap(1000, 500);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    bmp.SetPixel(j, (int)Remap(i, 0, 4, bmp.Height - 1, 0), Color.Gray);
                }
            }

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    bmp.SetPixel((int)Remap(i, 0, 13, 0, bmp.Width - 1), j, Color.Gray);
                }
            }

            for (int i = 0; i < scores.Length; i++)
            {
                float weight = scores[i].score.weight;
                float stars = scores[i].leaderboard.stars;

                int x, y;
                x = (int)Remap(stars, 0, 13, 0, bmp.Width - 1);
                y = (int)Remap(weight, 0, 1, bmp.Height - 1, 0);

                int r = (int)Remap((DateTime.Now - scores[i].score.timeSet).Days, 0, 365, 255, 127);
                if (r < 0) r = 0;
                Color c = Color.FromArgb(r, 0, 0);

                if (i <= 10) c = Color.Cyan;
                else if (i <= 24) c = Color.Green;
                else if (i <= 50) c = Color.Orange;

                bmp.SetPixel(x, y, c);
            }

            string path = @$"{$@"{userDataPath}Playgraphs\"}{CleanFileName(player.name).TrimEnd()}\";
            Directory.CreateDirectory(path);
            bmp.Save($"{path}{player.scoreStats.totalRankedScore}weight.png");
            DebugPrint(DebugLevel.Advanced, $"Finished creating weight playgraph for {player.name}");
        }

        internal static int GetRank(Player player, bool local = false) => (player.rank != 0) ? (!local ? player.rank : player.countryRank) : ApproximateRankFromPP(player, local);

        internal static int ApproximateRankFromPP(Player player, bool local)
        {
            int page = 0, pageRank;
            string countryCode = !local ? "" : player.country;

            bool playerPPIsHigher = false;
            Player cPlayer = null;
            while (!playerPPIsHigher)
            {
                pageRank = page++ * 50;
                Player[] players = GetPlayersByRank(countryCode, pageRank + 1, pageRank + 50);
                for (int i = 0; i < players.Length; i++)
                {
                    cPlayer = players[i];
                    playerPPIsHigher = player.pp > cPlayer.pp;

                    if (playerPPIsHigher) break;
                }
            }

            int cPlayerRank = !local ? cPlayer.rank : cPlayer.countryRank;

            return cPlayerRank;
        }

    }
}