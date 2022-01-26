using Ouroboros_API.ScoreSaberClasses;
using static Ouroboros_API.DebugManager;
using static Ouroboros_API.Core;
using static Ouroboros_API.Queries;
using static Ouroboros_API.Sniping;
using static Ouroboros_API.Playlists;
using System;
using System.Linq;

namespace Ouroboros_API
{
    class Program
    {

        #region Variables & Main

        #region IDs

        public const long CptShini = 76561198074878770;
        public const long Sensei = 76561198400393482;
        public const long Kat = 76561197997028786;
        public const long Zorri = 76561198163644494;
        public const long Jonathan = 76561198216194272;
        public const long Eff3ct = 76561198188430027;
        public const long Guy = 76561198136806348;
        public const long Viking = 76561198299618436;
        public const long Coreh = 76561198198217778;
        public const long Dark = 76561198030407451;
        public const long Hawk = 76561198086326146;
        public const long Lasse = 76561198113078478;
        public const long Taoh = 76561197993806676;

        #endregion

        static void Main(string[] args)
        {
            Program program = new();
            program.Start();
        }

        #endregion

        void Start()
        {
            debugLevel = DebugLevel.Basic;
            Initialize(@"E:\Steam\steamapps\common\Beat Saber\Playlists\");
            DeletePlaylists();

            Player player = GetPlayerInfoFull(CptShini);
            GenerateOuroboros(0, 14);
            GenerateReqPlaylists(player, 0, 14);
            GenerateSnipeTimePlaylists(player, 92.5f, true);
        }

        #region Links test stuff
        /*
                void GenerateMapLinks(int topN, string countryCode, int from, int to)
                {
                    Dictionary<Connection, LinkInfo> links = new();

                    Player[] players = GetFilteredPlayers(countryCode, from, to, 0);
                    foreach (Player player in players)
                    {
                        Console.WriteLine(countryCode.Length > 0 ? player.countryRank : player.rank + " out of " + players.Length);
                        AddSongLinks(links, GetPlayerScores(player, topN, "top").playerScores);
                    }
                    links = links.OrderByDescending(l => l.Value.topScoresPos.Count).ToDictionary(pair => pair.Key, pair => pair.Value);

                    SaveLinks(countryCode + from + "-" + to, links);
                }

                void AddSongLinks(Dictionary<Connection, LinkInfo> links, PlayerScore[] scores)
                {
                    int length = scores.Length;

                    for (int i = 0; i < length; i++)
                    {
                        for (int j = 0; j < length; j++)
                        {
                            if (i != j)
                            {
                                Connection conn = new() { sourceMap = scores[i].leaderboard, destinationMap = scores[j].leaderboard };

                                if (links.TryGetValue(conn, out LinkInfo li))
                                {
                                    li.topScoresPos.Add(new int[2] { i, j });

                                    links[conn] = li;
                                }
                                else
                                {
                                    li = new()
                                    {
                                        score = 0,
                                        topScoresPos = new List<int[]>()
                                    };
                                    li.topScoresPos.Add(new int[2] { i, j });
                                    links.Add(conn, li);
                                }
                            }
                        }
                    }
                }

                Dictionary<Connection, LinkInfo> CalculateLinkScores(Dictionary<Connection, LinkInfo> links, int offset)
                {
                    foreach (KeyValuePair<Connection, LinkInfo> link in links)
                    {
                        foreach (int[] item in link.Value.topScoresPos)
                        {
                            link.Value.score += MathF.Max(20 - MathF.Abs(item[0] - offset - item[1]), 1);
                        }
                        link.Value.score /= link.Value.topScoresPos.Count;
                    }

                    return links.OrderByDescending(l => l.Value.score).ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                Dictionary<LeaderboardInfo, float> GetScoredMaps(Dictionary<Connection, LinkInfo> links, PlayerScore[] scores)
                {
                    Dictionary<LeaderboardInfo, float> scoredMaps = new();

                    foreach (KeyValuePair<Connection, LinkInfo> item in GetFilteredLinksForPlayer(links, scores))
                    {
                        LeaderboardInfo key = item.Key.sourceMap;
                        if (scoredMaps.ContainsKey(key))
                        {
                            scoredMaps[key] += item.Value.score / scores.Length;
                        }
                        else
                        {
                            scoredMaps.Add(key, item.Value.score / scores.Length);
                        }
                    }
                    scoredMaps = scoredMaps.OrderByDescending(m => m.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

                    return scoredMaps;
                }

                Dictionary<Connection, LinkInfo> GetFilteredLinksForPlayer(Dictionary<Connection, LinkInfo> links, PlayerScore[] scores)
                {
                    Dictionary<Connection, LinkInfo> returnedlinks = new();

                    foreach (PlayerScore score in scores)
                    {
                        Dictionary<Connection, LinkInfo> filteredLinks = GetLinksByID(links, score.leaderboard.id);
                        foreach (KeyValuePair<Connection, LinkInfo> item in filteredLinks)
                        {
                            returnedlinks.Add(item.Key, item.Value);
                        }
                    }

                    returnedlinks.ToList().RemoveAll(l => scores.Any(s => s.leaderboard.id == l.Key.sourceMap.id));
                    return returnedlinks;
                }

                Dictionary<Connection, LinkInfo> GetLinksByID(Dictionary<Connection, LinkInfo> links, long targetID)
                {
                    Dictionary<Connection, LinkInfo> targetedLinks = new();

                    foreach (KeyValuePair<Connection, LinkInfo> item in links)
                    {
                        if (item.Key.destinationMap.id != targetID) { continue; }
                        targetedLinks.Add(item.Key, item.Value);
                    }

                    return targetedLinks;
                }

                void PrintLinks(Dictionary<Connection, LinkInfo> links, int n, int minimumApperances, float minimumScore, bool showOnlySource)
                {
                    int totalCount = NumResolve(n, links.Count);
                    int k = 0;
                    foreach (KeyValuePair<Connection, LinkInfo> item in links)
                    {
                        if (k > totalCount) { break; }
                        if (item.Value.topScoresPos.Count < minimumApperances || item.Value.score < minimumScore) { continue; }
                        PrintLink(item, showOnlySource);
                        k++;
                    }
                }

                void PrintLink(KeyValuePair<Connection, LinkInfo> item, bool showOnlySource)
                {
                    string result = "";

                    result += item.Value.score.ToString("00.0") + " | ";
                    result += item.Value.topScoresPos.Count.ToString("00") + "\t | ";
                    result += item.Key.sourceMap.stars.ToString("00.00") + "* -> ";
                    result += item.Key.destinationMap.stars.ToString("00.00") + "* | \t";
                    result += item.Key.sourceMap.songName + " (" + ResolveDifficultyName(item.Key.sourceMap.difficulty.difficulty) + ")";
                    result += showOnlySource ? "" : ("\t -> \t" + item.Key.destinationMap.songName + " (" + ResolveDifficultyName(item.Key.destinationMap.difficulty.difficulty) + ")");

                    Console.WriteLine(result);
                }
        */
        #endregion

    }
}