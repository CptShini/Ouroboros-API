using OuroborosLibrary.ScoreSaberClasses;

namespace OuroborosLibrary.Debugging
{
    public static class Printer
    {
        #region Console Printing

        public static void Print(object contents) => Console.Write(contents);

        public static void Println(object contents) => Console.WriteLine(contents);

        public static void Println() => Console.WriteLine();

        #endregion

        #region ScoreSaber class strings

        #region String Beautifiers

        private static string AWSAS(string s, int desiredLength)
        {
            string result = "";
            for (int i = 0; i < desiredLength - s.Length; i++)
            {
                result += " ";
            }

            return result + s;
        }

        private static string AWSAE(string s, int desiredLength)
        {
            string result = "";
            for (int i = 0; i < desiredLength - s.Length; i++)
            {
                result += " ";
            }

            return s + result;
        }

        private static string Truncate(string value, int maxLength) => string.IsNullOrEmpty(value) ? value : (value.Length <= maxLength ? value : value[..maxLength]);

        #endregion

        #region Attribute Strings

        #region PlayerScore

        private static string AccString(this PlayerScore ps) => $"{AWSAS($"{ps.accuracy:00.00}%", 7)} {AWSAS($"{ps.score.rank}#", 6)}  {AWSAS($"{ps.relativeRank:0.00}%#", 7)}";

        #endregion

        #region Player

        private static string RankString(this Player player) => $"{AWSAS($"{player.rank}#", 7)} {AWSAS($"({player.countryRank}#", 6)})";
        private static string AvgRankedAccString(this Player player) => $"{player.scoreStats.averageRankedAccuracy:00.00}%";
        private static string RankedPlayCountString(this Player player) => AWSAS($"{player.scoreStats.rankedPlayCount}", 5);
        private static string PPString(this Player player) => AWSAS($"{player.pp:0}pp", 8);
        private static string NameString(this Player player) => player.name;

        #endregion

        #region Score

        private static string PPString(this Score score) => $"{score.pp:0.00}pp ({score.pp * score.weight:0.00}pp)";
        private static string FCString(this Score score) => AWSAE(score.fullCombo ? "FC" : $"{score.maxCombo} ({score.missedNotes + score.badCuts}x)", 12);
        private static string ScoreString(this Score score) => AWSAE($"{AWSAS($"{score.baseScore}", 7)}{(score.modifiers.Length != 0 ? $" {score.modifiers}" : "")}", 13);
        private static string AgeString(this Score score) => AWSAS($"{(DateTime.Now - score.timeSet).TotalDays:0.0} days", 10);
        private static string NameString(this Score score) => score.leaderboardPlayerInfo.name;

        #endregion

        #region Leaderboard

        private static string StarsString(this LeaderboardInfo lb) => AWSAS($"{lb.stars:0.00}*", 7);
        private static string IdString(this LeaderboardInfo lb) => AWSAS(lb.id.ToString(), 7);
        private static string DiffString(this LeaderboardInfo lb) => AWSAE(lb.DifficultyName, 10);
        private static string NameString(this LeaderboardInfo lb) => $"{lb.songName} {(lb.songSubName.Length != 0 ? $"{lb.songSubName} " : "")}";
        private static string ScoreString(this LeaderboardInfo lb) => AWSAS(lb.maxScore.ToString(), 9);
        private static string RankedDateString(this LeaderboardInfo lb) => Truncate(lb.rankedDate.ToString(), 10);
        private static string LevelAuthorString(this LeaderboardInfo lb) => AWSAE(Truncate(lb.levelAuthorName, 20), 20);
        private static string SongAuthorString(this LeaderboardInfo lb) => AWSAE(Truncate(lb.songAuthorName, 20), 20);

        #endregion

        #endregion

        #region PlayerScore

        public static void PrintPlayerScores(this IEnumerable<PlayerScore> playerScores)
        {
            foreach (PlayerScore playerScore in playerScores)
            {
                playerScore.PrintPlayerScore();
            }
        }

        public static void PrintPlayerScore(this PlayerScore ps)
        {
            LeaderboardInfo lb = ps.leaderboard;
            Score score = ps.score;

            string result =
                $"{lb.StarsString()}\t | " +
                $"{score.PPString()}\t | " +
                $"{score.FCString()}" +
                $"{score.ScoreString()} | " +
                $"{ps.AccString()}   | " +
                $"{score.AgeString()} | " +
                $"{lb.IdString()} | " +
                $"{lb.DiffString()}\t | " +
                $"{lb.NameString()}";

            Println(result);
        }

        #endregion

        #region Player

        public static void PrintPlayers(this IEnumerable<Player> players)
        {
            foreach (Player player in players)
            {
                player.PrintPlayer();
            }
        }

        public static void PrintPlayer(this Player player)
        {
            string result =
                $"{player.RankString()} | " +
                $"{player.AvgRankedAccString()} | " +
                $"{player.RankedPlayCountString()} | " +
                $"{player.PPString()} | " +
                $"{player.NameString()}";

            Println(result);
        }

        #endregion

        #region Score

        public static void PrintScores(this IEnumerable<Score> scores)
        {
            foreach (Score score in scores)
            {
                score.PrintScore();
            }
        }

        public static void PrintScore(this Score score)
        {
            string result =
                $"{score.PPString()}\t | " +
                $"{score.FCString()}\t " +
                $"{score.ScoreString()} | " +
                $"{score.AgeString()}\t | " +
                $"{score.NameString()}";

            Println(result);
        }

        #endregion

        #region LeaderboardInfo

        public static void PrintLeaderboards(this IEnumerable<LeaderboardInfo> lbs)
        {
            foreach (LeaderboardInfo lb in lbs)
            {
                lb.PrintLeaderboard();
            }
        }

        public static void PrintLeaderboard(this LeaderboardInfo lb)
        {
            string result =
                $"{lb.StarsString()}\t | " +
                $"{lb.ScoreString()}\t | " +
                $"{lb.RankedDateString()}\t | " +
                $"{lb.IdString()} | " +
                $"{lb.DiffString()}\t | " +
                $"{lb.LevelAuthorString()}\t | " +
                $"{lb.SongAuthorString()}\t | " +
                $"{lb.NameString()}";

            Println(result);
        }

        #endregion

        #endregion
    }
}