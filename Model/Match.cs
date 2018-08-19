using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ochs
{
    public class Match 
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Location { get; set; }
        public virtual Person FighterRed { get; set; }
        public virtual Person FighterBlue { get; set; }
        public virtual int ScoreRed { get; set; }
        public virtual int ScoreBlue { get; set; }
        public virtual int ExchangeCount { get; set; }
        public virtual int DoubleCount { get; set; }
        public virtual int Round { get; set; } = 1;
        public virtual MatchResult Result { get; set; } = MatchResult.None;
        public virtual TimeSpan Time { get; set; }
        public virtual DateTime? TimeRunningSince { get; set; }
        public virtual DateTime? PlannedDateTime { get; set; }
        public virtual bool Planned => PlannedDateTime.HasValue;
        public virtual DateTime? StartedDateTime { get; set; }
        public virtual bool Started => StartedDateTime.HasValue;
        public virtual DateTime? FinishedDateTime { get; set; }
        public virtual bool Finished => FinishedDateTime.HasValue;
        public virtual DateTime? ValidatedDateTime { get; set; }
        public virtual bool Validated => ValidatedDateTime.HasValue;
        public virtual IList<MatchEvent> Events { get; set; } = new List<MatchEvent>();
        public virtual Competition Competition { get; set; }
        public virtual Phase Phase { get; set; }
        public virtual Pool Pool { get; set; }
        public virtual TimeSpan LiveTime => Time + (TimeRunningSince.HasValue ? DateTime.Now.Subtract(TimeRunningSince.Value) : TimeSpan.Zero);

        public virtual string GetLocation()
        {
                return !string.IsNullOrWhiteSpace(Location)? Location
                    : (!string.IsNullOrWhiteSpace(Pool?.Location) ? Pool.Location : Phase?.Location);
        }

        public virtual void UpdateMatchData()
        {
            ExchangeCount = Events.Count(x =>
                x.Type == MatchEventType.Score || x.Type == MatchEventType.AfterBlow ||
                x.Type == MatchEventType.DoubleHit || x.Type == MatchEventType.UnclearExchange);
            DoubleCount = Events.Count(x => x.Type == MatchEventType.DoubleHit);
            ScoreRed = Events.Sum(x => x.PointsRed < 0 ? x.PointsRed : (x.PointsRed > x.PointsBlue ? x.PointsRed - x.PointsBlue : 0));
            ScoreBlue = Events.Sum(x => x.PointsBlue < 0 ? x.PointsBlue : (x.PointsBlue > x.PointsRed ? x.PointsBlue - x.PointsRed : 0));
        }
    }

    public enum MatchResult
    {
        None,
        WinRed,
        WinBlue,
        Draw,
        ForfeitRed,
        ForfeitBlue,
        DisqualificationBlue,
        DisqualificationRed
    }
}