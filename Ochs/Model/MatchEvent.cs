using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ochs
{
    public class MatchEvent
    {
        public virtual Guid Id { get; set; }
        public virtual int Round { get; set; }
        public virtual DateTime CreatedDateTime { get; set; }
        public virtual DateTime? ChangedDateTime { get; set; }
        public virtual TimeSpan MatchTime { get; set; }
        public virtual MatchEventType Type { get; set; }
        public virtual int PointsRed { get; set; }
        public virtual int PointsBlue { get; set; }
        public virtual Match Match { get; set; }

        public virtual bool IsExchange => Type == MatchEventType.Score || Type == MatchEventType.Afterblow ||
                                          Type == MatchEventType.DoubleHit || Type == MatchEventType.UnclearExchange;
        public virtual string Note { get; set; }
    }

    public enum MatchEventType
    {
        Score,
        WarningRed,
        WarningBlue,
        Penalty,
        DoubleHit,
        Afterblow,
        UnclearExchange,
        ProtestBlue,
        ProtestRed,
        SportsmanshipBlue,
        SportsmanshipRed
    }
}