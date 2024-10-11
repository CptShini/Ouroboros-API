using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.Legacy.DebugManager;
using static Ouroboros_API.Legacy.Core;
using static Ouroboros_API.Legacy.Queries;
using static Ouroboros_API.Legacy.Playlists;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ouroboros_API.Legacy
{
    public static class SongSuggest
    {
        public static void GenerateSongSuggest(Player player, float percentile)
        {
            PlayerScore[] userScores = GetPlayerScores(player, -1);
            float[] scoresAcc = userScores.Take(50).Select(ps => ps.accuracy).ToArray();
            float targetAccuracy = Percentile(scoresAcc, percentile);

            DebugPrint(DebugLevel.None, $"Generating song suggestion playlist for {player.name} at {targetAccuracy:0.00}% avg acc{(Core.Config.SsRemoveAlreadyBeat ? " removing maps with better plays" : "")}");

            List<PlayerScore[]> playerScores = new List<PlayerScore[]>();
            int rank = GetRank(player);
            Player[] players = GetPlayersByRank("", (int)(rank / 1.2f), (int)(rank * 1.2f));

            DebugPrint(DebugLevel.Basic, $"Gettings top 50 scores for {players.Length} players; T-{GetTimeEstimate(players.Length, 460)}");
            players.ToList().ForEach(p => playerScores.Add(GetPlayerScores(p, 50)));

            List<ScoredMap> mapList = GetScoredMaps(playerScores, targetAccuracy);
            LeaderboardInfo[] maps = mapList.Select(map => map.Map).ToArray();

            if (Core.Config.SsRemoveAlreadyBeat) maps = maps.Where(lb => userScores.Where(ps => ps.accuracy > targetAccuracy - 0.25f || ps.score.fullCombo && ps.accuracy > targetAccuracy).All(ps => ps.leaderboard.id != lb.id)).ToArray();

            GenerateBPList($"Ø Top 100 maps for {player.name} @ {targetAccuracy:00.00}%", @"Sniping\", maps.Take(100).ToArray());
            DebugPrint(DebugLevel.Basic, "Finished generating song suggestion playlist\n");
        }

        private static List<ScoredMap> GetScoredMaps(List<PlayerScore[]> scoreList, float targetAccuracy)
        {
            List<ScoredMap> mapList = PopulateScoredMapList(scoreList, targetAccuracy);

            mapList = EvaluateScoredMapList(mapList);

            return mapList;
        }

        private static List<ScoredMap> PopulateScoredMapList(List<PlayerScore[]> scoreList, float targetAccuracy)
        {
            Dictionary<LeaderboardInfo, ScoredMap> mapDic = new();

            //string result = "";
            foreach (PlayerScore[] scores in scoreList)
            {
                for (int i = 0; i < scores.Length; i++)
                {
                    float deltaAcc = MathF.Abs(scores[i].accuracy - targetAccuracy);
                    if (deltaAcc > ScoredMap.AccCutoff) continue;

                    //result += $"{scores[i].leaderboard.songNameWDiff} ({scores[i].leaderboard.id})={i}={scores[i].accuracy}\n";

                    LeaderboardInfo map = scores[i].leaderboard;

                    bool mappedAlreadyAdded = mapDic.ContainsKey(map);
                    if (mappedAlreadyAdded)
                    {
                        mapDic[map].AddScore(deltaAcc, i);
                    }
                    else
                    {
                        ScoredMap scoredMap = new(map);
                        scoredMap.AddScore(deltaAcc, i);
                        mapDic.Add(map, scoredMap);
                    }
                }
            }
            //Save(@"C:\Users\gabri\Desktop\Test.txt", result);
            
            return mapDic.Values.ToList();
        }

        private static List<ScoredMap> EvaluateScoredMapList(List<ScoredMap> mapList) => mapList.OrderByDescending(m => m.GetMapScore()).ToList();

        private static float Percentile(float[] sequence, float excelPercentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;
            float n = (N - 1) * excelPercentile + 1;

            if (n == 1f) return sequence[0];
            else if (n == N) return sequence[N - 1];
            else
            {
                int k = (int)n;
                float d = n - k;
                return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
            }
        }
    }

    internal class ScoredMap
    {
        private const float PositionVal = 1.045f;
        private const float AccVal = 0.5f;
        public const float AccCutoff = 1.3f;
        private const float AmountVal = 0.96f;

        internal readonly LeaderboardInfo Map;

        private readonly List<float> _scores;

        internal ScoredMap(LeaderboardInfo map)
        {
            Map = map;
            _scores = new();
        }

        internal void AddScore(float deltaAcc, int position) => _scores.Add(ScoreMap(deltaAcc, position));

        internal float GetMapScore()
        {
            float[] scores = _scores.OrderByDescending(s => s).ToArray();
            for (int i = 0; i < scores.Length; i++)
            {
                scores[i] *= ScoreIndex(i);
            }

            return scores.Sum();
        }

        private static float ScoreMap(float deltaAccuracy, int position) => ScorePosition(position) * ScoreAccuracy(deltaAccuracy);
        private static float ScorePosition(int position) => MathF.Pow(PositionVal, -position) * 50;
        private static float ScoreAccuracy(float deltaAccuracy) => 1 - MathF.Pow(deltaAccuracy / AccCutoff, AccVal);
        private static float ScoreIndex(int i) => MathF.Pow(AmountVal, i);
    }
}