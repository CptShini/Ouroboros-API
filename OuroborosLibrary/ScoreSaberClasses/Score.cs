namespace OuroborosLibrary.ScoreSaberClasses
{
    /// <summary>
    /// A class containing all the information relevant to a score.
    /// </summary>
    public class Score
    {

        /// <summary>
        /// The ID of the score. I uhhh... I have no fucking idea where to ever use this.
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// Information regarding the player who set the score.
        /// </summary>
        public LeaderboardPlayer leaderboardPlayerInfo { get; set; }

        /// <summary>
        /// The rank on the leaderboards this score is.
        /// </summary>
        public int rank { get; set; }

        /// <summary>
        /// The score of the score.
        /// </summary>
        public int baseScore { get; set; }

        /// <summary>
        /// The score of the score after modifiers have been applied.
        /// </summary>
        public int modifiedScore { get; set; }

        /// <summary>
        /// The raw PP the play gave.
        /// </summary>
        public float pp { get; set; }

        /// <summary>
        /// The factor by which the raw PP was multiplied before being added to the players total PP.
        /// </summary>
        public float weight { get; set; }

        /// <summary>
        /// Any modifiers used during the play.
        /// </summary>
        public string modifiers { get; set; }

        /// <summary>
        /// The multiplier of the modifiers used.
        /// </summary>
        public float multiplier { get; set; }

        /// <summary>
        /// The number of bad cuts.
        /// </summary>
        public int badCuts { get; set; }

        /// <summary>
        /// The number of missed notes.
        /// </summary>
        public int missedNotes { get; set; }

        /// <summary>
        /// The max combo achieved during the play.
        /// </summary>
        public int maxCombo { get; set; }

        /// <summary>
        /// Whether or not the play was a full combo.
        /// </summary>
        public bool fullCombo { get; set; }

        /// <summary>
        /// What headset the player was using.
        /// </summary>
        public int hmd { get; set; }

        /// <summary>
        /// What time the play was set.
        /// </summary>
        public DateTime timeSet { get; set; }

        /// <summary>
        /// Whether or not the play's replay is available.
        /// </summary>
        public bool hasReplay { get; set; }

    }
}
