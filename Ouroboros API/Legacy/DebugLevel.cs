namespace Ouroboros_API.Legacy
{
    /// <summary>
    /// A class containing the shared debug level.
    /// </summary>
    public static class DebugManager
    {
        /// <summary>
        /// The level at which you desire debug information.
        /// </summary>
        public static DebugLevel debugLevel = DebugLevel.Basic;
    }

    /// <summary>
    /// A level deciding at what level debug information should be displayed.
    /// </summary>
    public enum DebugLevel { None, Basic, Advanced, Full, Dev }
}
