using System;
using System.Collections.Generic;
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