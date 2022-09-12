using System;

namespace Ouroboros_API.ScoreSaberClasses
{
    public class Connection : IEquatable<Connection>
    {
        public LeaderboardInfo sourceMap;
        public LeaderboardInfo destinationMap;

        public bool Equals(Connection conn)
        {
            return sourceMap.id == conn.sourceMap.id && destinationMap.id == conn.destinationMap.id;
        }

        public override int GetHashCode()
        {
            return sourceMap.id.GetHashCode() ^ destinationMap.id.GetHashCode();
        }
    }
}