using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using static Ouroboros_API.Sniping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

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

            if (debugLevel >= DebugLevel.None) Println($"Generating Snipe Time playlists for {sniper.name} {countryCode}{(local ? " " : "")}ranks {from} through {to}");

            if (debugLevel >= DebugLevel.Basic) Println("Retriving sniper top plays");
            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1);

            float acc = GetAverageAcc(sniperScores.Take(50).ToArray()) - 0.5f;

            if (debugLevel >= DebugLevel.Advanced) Println("Retriving players that fit criteria");
            Player[] filteredPlayers = GetFilteredPlayers(countryCode, from, to, acc, acc + 1f, true).Where(p => p.scoreStats.rankedPlayCount >= 100).Reverse().ToArray();

            if (debugLevel >= DebugLevel.Advanced) Println("Generating Snipe Time playlists for filtered players");

            Dictionary<Player, PlayerScore[]> playerScoreDictionary = new();
            foreach (Player player in filteredPlayers)
            {
                if (player.id == sniper.id) { if (debugLevel >= DebugLevel.Full) Println("Skipping over sniper"); continue; }
                PlayerScore[] plays = GetSnipedPlays(player, sniper, sniperScores, !frontPageSnipe, 50, false);
                
                if (plays.Length > 0) { playerScoreDictionary.Add(player, plays); } else if (debugLevel >= DebugLevel.Advanced) Println("No sniped plays, not adding player");
                if (playerScoreDictionary.Count >= n && n >= 0) break;
            }

            if (playerScoreDictionary.Count <= 0) { if (debugLevel >= DebugLevel.None) Println("No players with sniped maps found within criteria, returning"); return 0; }
            int k = 0;
            int highestRank = playerScoreDictionary.Keys.Max(p => local ? p.countryRank : p.rank);
            foreach (KeyValuePair<Player, PlayerScore[]> playerScorePair in playerScoreDictionary)
            {
                if (GenerateSnipeTimePlaylist(playerScorePair.Key, playerScorePair.Value, local, highestRank, CalculatePPGainForRaw(playerScorePair.Value, sniperScores))) k++;
            }

            if (debugLevel >= DebugLevel.None) Println("Finished generating Snipe Time playlists\n");
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


            if (debugLevel >= DebugLevel.None) Println($"Generating Snipe Time playlist for {name}");

            return GenerateBPList(title, path, ConvertPlayerScoreToLeaderboard(snipedPlays));
        }

        public static int GenerateSnipeTargetsPlaylists(Player sniper)
        {
            if (debugLevel >= DebugLevel.None) Println($"Generating {Core.Config.snipeList.Length} snipe playlists for {Core.Config.playerName}");

            PlayerScore[] sniperScores = GetPlayerScores(sniper, -1);

            List<Player> snipeTargets = new List<Player>();
            Core.Config.snipeList.ToList().ForEach(id => snipeTargets.Add(GetPlayerInfoFull(id)));

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
                if (debugLevel >= DebugLevel.Basic) Println($"Generating Snipe Time playlist for {sniped.name}");

                string stats = $"({AddZeros((int)sniped.pp, highestPP)}pp | (#{AddZeros(sniped.rank, highestRank)}/#{sniped.countryRank}) @ {sniped.scoreStats.averageRankedAccuracy:00.00}%)";
                string identifier = $"{sniped.name}";
                string ppGain = $"({CalculatePPGainForRaw(snipeTargetsDic[sniped], sniperScores):0.00}pp)";

                string title = $"Ø {stats} {identifier} {ppGain}";

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


            if (debugLevel >= DebugLevel.None) Println($"Generating Snipe Time playlist for {sniped.name}");

            return GenerateBPList(title, path, ConvertPlayerScoreToLeaderboard(GetSnipedPlays(sniped, sniper, sniperScores, false, -1, true)));
        }

        /// <summary>
        /// Creates a playlist containing the top 50 plays of the given player.
        /// </summary>
        /// <param name="player">The player who's plays to get.</param>
        public static void GenerateTopPlaysPlaylist(Player player)
        {
            string name = player.name;

            if (debugLevel >= DebugLevel.Basic) Println($"Generating top plays playlist for {name}\n");

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
            if (debugLevel >= DebugLevel.None) Println("Generating Ouroboros playlists");

            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                GenerateOuroborosPlaylist(new StarRange(i, i + 1));
            }

            if (debugLevel >= DebugLevel.None) Println("Finished generating Ouroboros playlists\n");
        }

        /// <summary>
        /// Creates a playlist with all ranked maps within the given star range.
        /// </summary>
        /// <param name="stars">The range of stars within the maps must lie.</param>
        public static void GenerateOuroborosPlaylist(StarRange stars)
        {
            string path = @"#\";
            string title = stars.name;

            if (debugLevel >= DebugLevel.None) Println($"Generating Ouroboros {title} playlist");

            LeaderboardInfo[] leaderboards = GetLeaderboards(stars, 0);
            GenerateBPList(title, path, leaderboards);

            if (debugLevel >= DebugLevel.Advanced) Println($"Finished generating Ouroboros {title} playlist");
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
            if (debugLevel >= DebugLevel.None) Println("Generating Ouroboros Req playlists");

            if (debugLevel >= DebugLevel.Basic) Println($"Retriving {player.name}'s plays");
            PlayerScore[] scores = GetPlayerScores(player, -1);
            for (int i = minStars; i < MathF.Min(maxStars, 12); i++)
            {
                int n = 0;
                StarRange stars = new StarRange(i, i + 1);
                LeaderboardInfo[] maps = GetLeaderboards(stars, 0);
                PlayerScore[] starScores = scores.Where(ps => maps.Any(m => m.id == ps.leaderboard.id)).ToArray();

                if (starScores.Length == 0) continue;

                if (Core.Config.FCReq)
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

                if (debugLevel == DebugLevel.None) Println($"Generating Ouroboros Req {stars.name} playlist");

                if (GenerateAbsoluteRankReqPlaylist(starScores, stars)) n++;
                if (GenerateAbsoluteAccReqPlaylist(starScores, stars)) n++;
                if (GenerateRelativeRankReqPlaylist(starScores, stars)) n++;
                if (GenerateRelativeAccReqPlaylist(starScores, stars)) n++;
                if (GenerateHighestPPPlaylist(starScores, stars)) n++;


                GenerateEmptyPlaylists(n, $"{stars.name}", @"Øuroboros\");
                if (debugLevel >= DebugLevel.Advanced) Println($"Finished generating Ouroboros Req {stars.name} playlist");
            }

            if (debugLevel >= DebugLevel.None) Println("Finished generating Ouroboros Req playlists\n");
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

            string title = $"{stars.name} Best plays";
            if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {n}");
            
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(playerScores).Take(n).ToArray());
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
            if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {n}");

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

            if (Core.Config.splitByElderTech) maps = SplitByEldertech(maps);

            string title = $"{stars.name} not played";
            if (GenerateBPList(title, @"Øuroboros\", maps))
            {
                if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {maps.Length}");
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
                if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {playerScores.Length}");
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
            if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {scores.Count}");
            return GenerateBPList(title, @"Øuroboros\", ConvertPlayerScoreToLeaderboard(SplitPlays(scores.ToArray())));
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
            if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {scores.Count}");
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
            if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {scores.Count}");
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
            if (debugLevel >= DebugLevel.Basic) Println($"Generating Ouroboros Req playlist {title} of length {scores.Count}");
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

        #endregion

        public static void GenerateDominancePlaylist(Player player)
        {
            string countryCode = player.country;

            if (debugLevel >= DebugLevel.None) Println($"Generating dominance playlist in {countryCode}");

            List<PlayerScore> playerScores = new List<PlayerScore>();
            PlayerScore[] sniperScores = GetPlayerScores(player, -1);
            
            Player[] players = GetPlayersByRank(countryCode, 1, -1).Reverse().ToArray();

            if (debugLevel >= DebugLevel.Basic) Println($"Gettings all ranked scores for {players.Length} players in {countryCode}");

            int prevCount = 0;
            int minRank = 3000;
            for (int i = 0; playerScores.Count < 20; i++)
            {
                prevCount = playerScores.Count;

                Player sniped = players[i];
                PlayerScore[] notSnipedScores = GetSnipedPlays(sniped, player, sniperScores, false, -1, true);
                for (int j = 0; j < notSnipedScores.Length; j++)
                {
                    PlayerScore score = notSnipedScores[j];
                    PlayerScore listScore = playerScores.Find(ps => ps.leaderboard.id == score.leaderboard.id);
                    if (listScore != null)
                    {
                        if (score.accuracy > listScore.accuracy)
                        {
                            playerScores.Remove(listScore);
                            playerScores.Add(score);
                        }
                    }
                    else playerScores.Add(score);
                }

                if (playerScores.Count > prevCount) minRank = sniped.countryRank;
            }

            string title = $"Dominance in {countryCode} @ #{minRank}";

            playerScores = playerScores.OrderBy(ps => ps.leaderboard.stars).ToList();
            LeaderboardInfo[] maps = SplitByEldertech(ConvertPlayerScoreToLeaderboard(playerScores.ToArray()));
            GenerateBPList(title, @"Øuroboros\", maps);

            if (debugLevel >= DebugLevel.Advanced) Println($"Finished generating dominance playlist in {countryCode}");
        }

        public static void GeneratePlayGraphs()
        {
            Player player = GetPlayerInfoFull(Core.Config.playerId);
            SavePlayGraphBMP(player);

            for (int i = 0; i < Core.Config.snipeList.Length; i++)
            {
                Player p = GetPlayerInfoFull(Core.Config.snipeList[i]);
                SavePlayGraphBMP(p);
            }
        }

        private static void SavePlayGraphBMP(Player player)
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
            bmp.Save($"{path}{player.scoreStats.totalRankedScore}.png");
            if (debugLevel >= DebugLevel.Advanced) Println($"Finished creating playgraph for {player.name}");
        }

    }
}