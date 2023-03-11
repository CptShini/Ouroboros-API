using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Ouroboros_API
{
    public static class SongSuggest
    {
        public static void GenerateSongSuggest(Player player, float offset)
        {
            PlayerScore[] userScores = GetPlayerScores(player, -1);
            float targetAccuracy = GetAverageAcc(userScores.Take(50).ToArray()) + offset;

            if (debugLevel >= DebugLevel.None) Println($"Generating song suggestion playlist for {player.name} at {targetAccuracy:0.00}% avg acc{(Core.Config.ssRemoveAlreadyBeat ? " removing maps with better plays" : "")}");

            List<PlayerScore[]> playerScores = new List<PlayerScore[]>();
            Player[] players = GetPlayersByRank("", (int)(player.rank / 1.2f), (int)(player.rank * 1.2f));

            if (debugLevel >= DebugLevel.Basic) Println($"Gettings top 50 scores for {players.Length} players; T-{GetTimeEstimate(players.Length, 460)}");
            players.ToList().ForEach(p => playerScores.Add(GetPlayerScores(p, 50)));

            List<ScoredMap> mapList = GetScoredMaps(playerScores, targetAccuracy);
            LeaderboardInfo[] maps = mapList.Select(map => map.map).ToArray();
            if (Core.Config.ssRemoveAlreadyBeat) maps = maps.Where(lb => !userScores.Where(ps => ps.accuracy > targetAccuracy - 0.5f).Any(ps => ps.leaderboard.id == lb.id)).ToArray();

            GenerateBPList($"Top 100 maps for {player.name} @ {targetAccuracy:00.00}%", @"Øuroboros\", maps.Take(100).ToArray());
            if (debugLevel >= DebugLevel.Basic) Println("Finished generating song suggestion playlist\n");
        }

        private static List<ScoredMap> GetScoredMaps(List<PlayerScore[]> scoreList, float targetAccuracy)
        {
            List<ScoredMap> mapList = PopulateScoredMapList(scoreList, targetAccuracy);

            mapList = EvaluteScoredMapList(mapList);

            return mapList;
        }

        private static List<ScoredMap> PopulateScoredMapList(List<PlayerScore[]> scoreList, float targetAccuracy)
        {
            List<ScoredMap> mapList = new List<ScoredMap>();

            foreach (PlayerScore[] scores in scoreList)
            {
                for (int i = 0; i < scores.Length; i++)
                {
                    float d = MathF.Abs(scores[i].accuracy - targetAccuracy);
                    if (d > 2) continue;

                    ScoredMap m;
                    LeaderboardInfo lb = scores[i].leaderboard;
                    if (mapList.Exists(map => map.map.id == lb.id))
                    {
                        m = mapList.Where(map => map.map.id == lb.id).First();
                        mapList.Remove(m);
                    }
                    else { m = new ScoredMap() { map = lb, count = 0, scores = new List<float>() }; }

                    m.count++;
                    m.scores.Add(ScoreMap(d, i));
                    mapList.Add(m);
                }
            }

            return mapList;
        }

        private static List<ScoredMap> EvaluteScoredMapList(List<ScoredMap> mapList)
        {
            foreach (ScoredMap map in mapList)
            {
                map.scores = map.scores.OrderByDescending(s => s).ToList();
                for (int i = 0; i < map.scores.Count; i++)
                {
                    map.scores[i] *= MathF.Pow(0.955f, i);
                }

                map.score = map.scores.Sum();
            }

            return mapList.OrderByDescending(m => m.score).ToList();
        }

        private static float ScoreMap(float deltaAccuracy, int position)
        {
            float positionScore = MathF.Pow(1.05f, -position) * 50;
            //float accuracyScore = 0.2f * MathF.Pow(deltaAccuracy, 2) - 0.9f * deltaAccuracy + 1;
            float accuracyScore = 1 - MathF.Sqrt(deltaAccuracy) / 1.41421356237f;
            return positionScore * accuracyScore;
        }
    }

    internal class ScoredMap
    {
        public LeaderboardInfo map;

        public int count;

        public List<float> scores;
        public float score;
    }
}